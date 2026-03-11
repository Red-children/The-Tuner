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
        // 自动获取子模块（确保挂载在同一对象）
        AutoGetSubModules();
        // 初始化所有子模块
        InitAllSubModules();
        // 订阅播放事件
        PreciseEventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);
    }

    private void OnDestroy()
    {
        // 清理资源
        StopAllCoroutines();
        PreciseEventBus.Instance.Unsubscribe<PlayBGMEvent>(OnPlayBGM);
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
        if (_songData == null || _progressManager == null || _multiplierManager == null || _indicatorManager == null)
        {
            Debug.LogError("PreciseBGMController: 缺少子模块！请确保所有子模块挂载在同一对象上");
        }
    }

    // 初始化所有子模块
    private void InitAllSubModules()
    {
        _progressManager?.Init(_songData);
        _multiplierManager?.Init(_songData, _progressManager);
        _indicatorManager?.Init(_songData, _progressManager);
        Debug.Log("PreciseBGMController: 所有子模块初始化完成");
    }
    #endregion

    #region 事件回调
    // 接收播放BGM事件
    private void OnPlayBGM(PlayBGMEvent evt)
    {
        Debug.Log("PreciseBGMController: 接收到播放BGM事件");
        // 启动BGM播放
        _progressManager?.StartBgmPlay();
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
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(interval); // 不受TimeScale影响

        while (_progressManager != null && _progressManager.IsPlaying)
        {
            // 1. 更新精准进度
            _progressManager.UpdatePreciseProgress();
            // 2. 计算并推送倍率更新
            _multiplierManager.CalculateAndPushMultiplier();
            // 3. 检查并推送指示器
            _indicatorManager.CheckAndPushIndicator();

            // 检查是否播放完成
            if (_progressManager.IsBgmFinished())
            {
                _progressManager.StopBgmPlay();
                _indicatorManager.ResetIndicatorState();
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