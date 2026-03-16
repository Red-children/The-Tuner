using System.Collections;
using UnityEngine;

// BGM主控模块（整合所有子模块，统一调度）
public class PreciseBGMController : MonoBehaviour
{
    // 子模块引用（挂载在同一对象上）
    [SerializeField] private BgmSongData _songData;
    [SerializeField] private BgmProgressManager _progressManager;
    [SerializeField] private BgmMultiplierManager _multiplierManager;
    [SerializeField] private BgmIndicatorManager _indicatorManager;

    private Coroutine _progressSamplerCoroutine; // 进度采样协程

    #region 生命周期
    private void Awake()
    {
        AutoGetSubModules();
        // 只初始化进度管理器，倍率和指示器模块禁用
        _progressManager?.Init(_songData);
        // _multiplierManager?.Init(_songData, _progressManager);  // 注释
        // _indicatorManager?.Init(_songData, _progressManager);    // 注释

        // 订阅播放事件（使用 EventBus，后文会统一）
        EventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        EventBus.Instance.Unsubscribe<PlayBGMEvent>(OnPlayBGM);  // 改为 EventBus
        _progressManager?.StopBgmPlay();
        _indicatorManager?.ResetIndicatorState();
    }

    #endregion

    #region 模块初始化
    // 自动获取子模块（避免手动赋值）
    private void AutoGetSubModules()
    {
        if (_songData == null) _songData = GetComponent<BgmSongData>();
        if (_progressManager == null) _progressManager = GetComponent<BgmProgressManager>();
        if (_multiplierManager == null) _multiplierManager = GetComponent<BgmMultiplierManager>();
        if (_indicatorManager == null) _indicatorManager = GetComponent<BgmIndicatorManager>();

        // 检查子模块是否齐全
        // if (_songData == null || _progressManager == null || _multiplierManager == null || _indicatorManager == null)
        if (_songData == null || _progressManager == null || _multiplierManager == null)
        {
            Debug.LogError("PreciseBGMController: 缺少子模块！请确保所有子模块挂载在同一对象上");
        }
    }

    #endregion

    #region 事件回调
    // 接收播放BGM事件
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
            RhythmManager.Instance.StartRhythm(dspStart, firstOffset);

            // 如果需要，同步 BPM
            // RhythmManager.Instance.bpm = (int)_songData.BPM;
            // 注意：修改 bpm 后需重新计算 beatInterval，可在 StartRhythm 中处理
        }

        // 启动进度采样协程
        if (_progressSamplerCoroutine != null) StopCoroutine(_progressSamplerCoroutine);
        _progressSamplerCoroutine = StartCoroutine(PreciseProgressSampler());
    }
    #endregion

    #region 高频采样逻辑
    // 高精度进度采样协程（替代Update，减少空判断）
    private IEnumerator PreciseProgressSampler()
    {
        if (_songData == null) yield break;

        float interval = _songData.sampleIntervalMs / 1000f;
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(interval);

        while (_progressManager != null && _progressManager.IsPlaying)
        {
            _progressManager.UpdatePreciseProgress();
            // _multiplierManager.CalculateAndPushMultiplier(); // 注释
            // _indicatorManager.CheckAndPushIndicator();       // 注释

            if (_progressManager.IsBgmFinished())
            {
                _progressManager.StopBgmPlay();
                // _indicatorManager.ResetIndicatorState();     // 注释
                // 可选：停止 RhythmManager
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
        _indicatorManager?.ResetIndicatorState();
        if (_progressSamplerCoroutine != null)
        {
            StopCoroutine(_progressSamplerCoroutine);
            _progressSamplerCoroutine = null;
        }
    }
    #endregion

    

}