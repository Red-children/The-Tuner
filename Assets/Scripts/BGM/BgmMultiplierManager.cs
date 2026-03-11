using UnityEngine;

// 倍率更新推送模块（仅处理倍率计算与事件推送）
public class BgmMultiplierManager : MonoBehaviour
{
    private BgmSongData _songData;       // 依赖歌曲配置
    private BgmProgressManager _progressManager; // 依赖进度模块
    private int _currentMultiplierIndex = 1; // 当前倍率索引（1=普通，0=精准）

    // 初始化（由主控调用）
    public void Init(BgmSongData songData, BgmProgressManager progressManager)
    {
        _songData = songData;
        _progressManager = progressManager;
        _currentMultiplierIndex = 1;
        Debug.Log("BgmMultiplierManager: 倍率模块初始化完成");
    }

    // 计算并推送倍率更新事件（由主控高频调用）
    public void CalculateAndPushMultiplier()
    {
        if (_songData == null || _progressManager == null || !_progressManager.IsPlaying)
            return;

        // 计算当前节拍偏移
        double beatInterval = 60 / _songData.BPM; // 每拍间隔（秒）
        double offset = _progressManager.PreciseTime % beatInterval; // 拍内偏移
        double timeToNextBeat = beatInterval - offset; // 距离下一拍的时间

        // 判断是否切换倍率（精准区间内=精准倍率，否则=普通倍率）
        int newIndex = timeToNextBeat < _songData.windowPeriod ? 0 : 1;

        // 倍率未变化则不推送事件
        if (newIndex == _currentMultiplierIndex) return;

        // 更新倍率索引并推送事件
        _currentMultiplierIndex = newIndex;
        PushMultiplierChangedEvent();
        Debug.Log($"BgmMultiplierManager: 倍率切换为{_songData.multiplierArray[_currentMultiplierIndex]}（索引={_currentMultiplierIndex}）");
    }

    // 推送倍率变更事件
    private void PushMultiplierChangedEvent()
    {
        var evt = new AttackMultiplierChangedEvent
        {
            newMultiplier = _songData.multiplierArray[_currentMultiplierIndex],
            isCritical = _currentMultiplierIndex == 0,
            time = _progressManager.PreciseTime
        };
        EventBus.Instance.Trigger<AttackMultiplierChangedEvent>(evt);
    }
}