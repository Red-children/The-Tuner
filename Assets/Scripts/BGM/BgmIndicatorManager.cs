using System.Collections.Generic;
using UnityEngine;

// 指示器推送模块（仅处理指示器时间戳与事件推送）
public class BgmIndicatorManager : MonoBehaviour
{
    [Header("【指示器配置】")]
    [Tooltip("指示器提前推送时间（秒）")]
    public const double publishAdavance = 0.3f;

    private BgmSongData _songData;               // 依赖歌曲配置
    private BgmProgressManager _progressManager; // 依赖进度模块
    private Queue<double> _beatTimestampQueue;   // 节拍时间戳队列
    private Queue<double> _indicatorPublishQueue; // 指示器推送时间戳队列
    private bool _hasPublishedCurrentPoint = true; // 避免重复推送

    // 初始化（由主控调用）
    public void Init(BgmSongData songData, BgmProgressManager progressManager)
    {
        _songData = songData;
        _progressManager = progressManager;
        _beatTimestampQueue = new Queue<double>();
        _indicatorPublishQueue = new Queue<double>();
        _hasPublishedCurrentPoint = true;

        // 预生成节拍和推送时间戳
        PreGenerateTimestamps();
        Debug.Log($"BgmIndicatorManager: 指示器模块初始化完成，预生成节拍数={_beatTimestampQueue.Count}");
    }

    // 预生成所有节拍和指示器推送时间戳
    private void PreGenerateTimestamps()
    {
        if (_songData == null || _songData.BgmClip == null)
        {
            Debug.LogError("BgmIndicatorManager: 歌曲配置未初始化，无法生成时间戳！");
            return;
        }

        double beatInterval = 60 / _songData.BPM; // 每拍间隔（秒）
        double currentBeatTime = _songData.firstOffset; // 第一拍时间

        // 生成所有节拍的时间戳和推送时间戳
        while (currentBeatTime < _songData.BgmClip.length)
        {
            _beatTimestampQueue.Enqueue(currentBeatTime);
            double publishTime = currentBeatTime - publishAdavance;
            _indicatorPublishQueue.Enqueue(publishTime);
            currentBeatTime += beatInterval;
        }
        //  Test调试
        foreach(var ptime in _indicatorPublishQueue)
        {
            Debug.Log("预计算指示器时间戳：" + ptime);
        }
    }

    // 检查并推送指示器事件（由主控高频调用）
    public void CheckAndPushIndicator()
    {
        if (_songData == null || _progressManager == null || !_progressManager.IsPlaying)
            return;
        float preciseTime = _progressManager.PreciseTime;
        // 过滤：前N秒不推送 + 队列为空则返回
        if (preciseTime < _songData.firstOffset || _indicatorPublishQueue.Count == 0)
            return;
        // 获取首个待推送时间
        double targetPublishTime = _indicatorPublishQueue.Peek();
        // 时间匹配：到达推送时间（误差1ms内）
        // if ((targetPublishTime - preciseTime) < 1e-3)

        if (preciseTime >= targetPublishTime )
        {
            // 出队并推送事件
            double publishTime = _indicatorPublishQueue.Dequeue();
            double currentBeatTime = _beatTimestampQueue.Dequeue();
            PushIndicatorEvent(currentBeatTime, preciseTime);
            _hasPublishedCurrentPoint = true;

            Debug.Log($"BgmIndicatorManager: 推送指示器 | 节拍时间={currentBeatTime:F8} | 推送时间={preciseTime:F8} | 目标推送时间={publishTime:F8}");
        }
    }

    // 推送指示器事件
    private void PushIndicatorEvent(double nextPoint, double currentTime)
    {
        var evt = new IndicatorActiveEvent
        {
            advance = publishAdavance,
            time = currentTime,
            nextPoint = nextPoint
        };
        PreciseEventBus.Instance.Trigger<IndicatorActiveEvent>(evt);
        Debug.Log($"推送指示器事件CurrentTime:{evt.time},nextPoint:{evt.nextPoint}");
    }

    // 重置指示器状态（停止播放时调用）
    public void ResetIndicatorState()
    {
        _beatTimestampQueue?.Clear();
        _indicatorPublishQueue?.Clear();
        _hasPublishedCurrentPoint = true;
        // 重新预生成时间戳（下次播放时用）
        if (_songData != null) PreGenerateTimestamps();
    }
}