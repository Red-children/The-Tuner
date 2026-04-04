using System.Collections;
using UnityEngine;

// BGM主控模块（整合所有子模块，统一调度）
public class PreciseBGMController : MonoBehaviour
{
    // 子模块引用（挂载在同一对象上）
    [SerializeField] private BgmSongData _songData;                 // 歌曲信息
    [SerializeField] private BgmProgressManager _progressManager;   // 进度管理器

    private Coroutine _progressSamplerCoroutine; // 进度采样协程

    #region 生命周期
    private void Awake()
    {
        //模块初始化
        AutoGetSubModules();        
        // 只初始化进度管理器，倍率和指示器模块禁用
        _progressManager?.Init(_songData);
       

        // 订阅播放事件
        EventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);  
    }

    private void OnDestroy()
    {
        //停止所有协程 
        StopAllCoroutines();
        EventBus.Instance.Unsubscribe<PlayBGMEvent>(OnPlayBGM);
        //摧毁时停止音乐
        _progressManager?.StopBgmPlay();
        
    }

    #endregion

    #region 模块初始化
    // 自动获取子模块（避免手动赋值）
    private void AutoGetSubModules()
    {
        if (_songData == null) _songData = GetComponent<BgmSongData>();
        if (_progressManager == null) _progressManager = GetComponent<BgmProgressManager>();
        
     
        // 检查子模块是否齐全
        // if (_songData == null || _progressManager == null || _multiplierManager == null || _indicatorManager == null)
        if (_songData == null || _progressManager == null)
        {
            Debug.LogError("PreciseBGMController: 缺少子模块！请确保所有子模块挂载在同一对象上");
        }
    }

    #endregion

    #region 事件回调
    // 接收播放BGM事件 得到开始播放的时间
    private void OnPlayBGM(PlayBGMEvent evt)
    {
        Debug.Log("PreciseBGMController: 接收到播放BGM事件");

        // 启动BGM播放
        _progressManager?.StartBgmPlay();

        // 同步节奏管理器
        if (RhythmManager.Instance != null && _songData != null)
        {
            double dspStart = _progressManager.DspStartTime; // 记录的音乐开始时间
            double firstOffset = _songData.firstOffset;       // 歌曲第一拍偏移（需在 BgmSongData 中配置）

            RhythmManager.Instance.bpm = (int)_songData.BPM; // 同步 BPM
            //通知实时计算
            RhythmManager.Instance.StartRhythm(dspStart, firstOffset);

            // 如果需要，同步 BPM
            // RhythmManager.Instance.bpm = (int)_songData.BPM;
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
        if (_songData == null) yield break;

        
        float interval = _songData.sampleIntervalMs / 1000f;//每拍的时间间隔 单位换算，把采样频率换算成ms 

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
    #endregion

    

}