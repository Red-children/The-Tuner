using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// bgm主控模块，负责协调歌曲数据、进度管理和节奏同步，挂载在PreciseBGMManager对象上
/// </summary>
public class PreciseBGMController : MonoBehaviour
{
    // 子模块引用（挂载在同一对象上）
    [SerializeField] private BgmSongData songData;                 // 歌曲信息
    [SerializeField] private BgmProgressManager _progressManager;   // 进度管理器

    [Header("音量控制")]
    [Tooltip("BGM默认播放音量（0-1）")]
    [SerializeField] private float bgmVolume = 0.1f;

    private Coroutine _progressSamplerCoroutine; // 进度采样协程
    private Tweener _fadeTween;                 // 淡入淡出Tween

    #region 生命周期
    private void Awake()
    {
        // 单例保护：如果已有持久化的实例，销毁重复的
        var existing = FindObjectOfType<PreciseBGMController>();
        if (existing != null && existing != this)
        {
            Destroy(gameObject);
            return;
        }

        //模块初始化
        AutoGetSubModules();        
        // 只初始化进度管理器，倍率和指示器模块禁用
        _progressManager?.Init(songData);
    
        // 订阅播放事件
        EventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);  
        
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        //停止所有协程 
        StopAllCoroutines();
        EventBus.Instance.Unsubscribe<PlayBGMEvent>(OnPlayBGM);
        KillFadeTween();
        //摧毁时停止音乐
        _progressManager?.StopBgmPlay();
        
    }

    #endregion

    #region 模块初始化
    // 自动获取子模块（避免手动赋值）
    private void AutoGetSubModules()
    {
        if (songData == null) songData = GetComponent<BgmSongData>();
        if (_progressManager == null) _progressManager = GetComponent<BgmProgressManager>();
        
     
        // 检查子模块是否齐全
        // if (songData == null || _progressManager == null || _multiplierManager == null || _indicatorManager == null)
        if (songData == null || _progressManager == null)
        {
            Debug.LogError("PreciseBGMController: 缺少子模块！请确保所有子模块挂载在同一对象上");
        }
    }

    #endregion

    #region 事件回调
    // 接收播放BGM事件 得到开始播放的时间
    private void OnPlayBGM(PlayBGMEvent evt)
    {
        // 如果已有BGM在播放，忽略新的播放请求（避免场景切换时BGMTrigger覆盖当前BGM）
        if (_progressManager != null && _progressManager.IsPlaying)
        {
            Debug.Log("PreciseBGMController: BGM已在播放中，忽略新的播放事件");
            return;
        }

        Debug.Log("PreciseBGMController: 接收到播放BGM事件");

        // 启动BGM播放
        _progressManager?.StartBgmPlay();

        // 应用自定义BGM音量（覆盖设置音量）
        if (songData?.BgmAudioSource != null)
            songData.BgmAudioSource.volume = bgmVolume;

        // 同步节奏管理器
        if (RhythmManager.Instance != null && songData != null)
        {
            double dspStart = _progressManager.DspStartTime; // 记录的音乐开始时间
            double firstOffset = songData.GetFirstOffset();       // 歌曲第一拍偏移（需在 BgmSongData 中配置）

            RhythmManager.Instance.bpm = songData.GetBPM(); // 同步 BPM
            //通知实时计算
            RhythmManager.Instance.StartRhythm(dspStart, firstOffset);

            // 如果需要，同步 BPM
            // RhythmManager.Instance.bpm = (int)songData.BPM;
            // 注意：修改 bpm 后需重新计算 beatInterval，可在 StartRhythm 中处理
        }

        // 启动进度采样协程 先停止旧的协程，避免重复启动
        if (_progressSamplerCoroutine != null) StopCoroutine(_progressSamplerCoroutine);
        _progressSamplerCoroutine = StartCoroutine(PreciseProgressSampler());

    }
    #endregion

    #region 高频采样逻辑
    // 高精度进度采样协程（替代Update，减少空判断）
    private IEnumerator PreciseProgressSampler()
    {
        if (songData == null) yield break;

        
        float interval = songData.sampleIntervalMs / 1000f;//每拍的时间间隔 单位换算，把采样频率换算成ms 

        //不受时间缩放影响的等待对象 绝对的时间
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(interval); 

        while (_progressManager != null && _progressManager.IsPlaying)
        {
            //更新歌曲播放进度 方法由BgmProgressManager提供，内部会计算当前播放时间和进度百分比，并触发相关事件
            _progressManager.UpdatePreciseProgress();   
            

            if (_progressManager.IsBgmFinished())
            {
                _progressManager.StopBgmPlay();
                // _indicatorManager.ResetIndicatorState();     // 注释
                // 停止 RhythmManager
                RhythmManager.Instance?.StopRhythm();
                break;
            }
            yield return wait;
        }
        _progressSamplerCoroutine = null;
    }
    #endregion

    #region 对外接口
    // 停止BGM（外部调用）
    public void StopBGM()
    {
        _progressManager?.StopBgmPlay();
        if (_progressSamplerCoroutine != null)
        {
            StopCoroutine(_progressSamplerCoroutine);
            _progressSamplerCoroutine = null;
        }
    }
    public void ChangeBGM(BGMData data)
    {
        StopBGM();
        songData.SwitchBGM(data);
        OnPlayBGM(new());
    }

    /// <summary> 淡出BGM（使用DOTween降低AudioSource音量） </summary>
    /// <param name="duration">淡出时长（秒）</param>
    /// <param name="onComplete">淡出完成回调</param>
    public void FadeOutBGM(float duration, Action onComplete = null)
    {
        if (songData == null || songData.BgmAudioSource == null) return;

        KillFadeTween();

        _fadeTween = songData.BgmAudioSource.DOFade(0f, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .SetTarget(gameObject)
            .OnComplete(() =>
            {
                StopBGM();
                // 恢复音量以便下次播放
                if (songData?.BgmAudioSource != null)
                    songData.BgmAudioSource.volume = bgmVolume;
                onComplete?.Invoke();
            });
    }

    /// <summary> 淡入BGM（先切换数据再淡入播放） </summary>
    /// <param name="data">新的BGM数据</param>
    /// <param name="duration">淡入时长（秒）</param>
    /// <param name="onComplete">淡入完成回调</param>
    /// <summary> 淡入BGM（先切换数据再淡入播放） </summary>
    /// <param name="data">新的BGM数据</param>
    /// <param name="duration">淡入时长（秒）</param>
    /// <param name="onComplete">淡入完成回调</param>
    public void FadeInBGM(BGMData data, float duration, Action onComplete = null)
    {
        if (songData == null || songData.BgmAudioSource == null) return;

        KillFadeTween();

        _progressManager?.StopBgmPlay();
        songData.SwitchBGM(data);

        songData.BgmAudioSource.volume = 0f;
        songData.BgmAudioSource.Play();
        _progressManager?.MarkAsPlaying();

        if (RhythmManager.Instance != null && songData != null)
        {
            double dspStart = _progressManager.DspStartTime;
            double firstOffset = songData.GetFirstOffset();
            RhythmManager.Instance.bpm = songData.GetBPM();
            RhythmManager.Instance.StartRhythm(dspStart, firstOffset);
        }

        if (_progressSamplerCoroutine != null) StopCoroutine(_progressSamplerCoroutine);
        _progressSamplerCoroutine = StartCoroutine(PreciseProgressSampler());

        _fadeTween = songData.BgmAudioSource.DOFade(bgmVolume, duration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .SetTarget(gameObject)
            .OnComplete(() => onComplete?.Invoke());
    }
    #endregion

    #region 内部方法
    private void KillFadeTween()
    {
        if (_fadeTween != null && _fadeTween.IsActive())
        {
            _fadeTween.Kill();
            _fadeTween = null;
        }
    }
    #endregion

    

}