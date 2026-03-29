using UnityEngine;

// 歌曲进程管理模块（仅处理播放状态和进度计算）
public class BgmProgressManager : MonoBehaviour
{
    // [Header("【进程信息】")]
    [Tooltip("是否正在播放")]
    public bool IsPlaying { get; private set; }
    [Tooltip("当前精准播放时间（秒）")]
    public float PreciseTime { get; private set; }
    [Tooltip("BGM开始播放时的DSP时间(音频硬件时间)")]
    public double DspStartTime { get; private set; }

    private BgmSongData _songData; // 依赖歌曲配置

    // 初始化（由主控调用）
    public void Init(BgmSongData songData)
    {
        _songData = songData;
        IsPlaying = false;
        PreciseTime = 0;
        DspStartTime = 0;
    }

    // 启动BGM播放（精准调度）
    public void StartBgmPlay()
    {
        if (_songData == null || _songData.BgmAudioSource == null || _songData.BgmClip == null)
        {
            Debug.LogError("BgmProgressManager: 歌曲配置或音频源未初始化！");
            return;
        }

        if (_songData.BgmAudioSource.isPlaying)
        {
            Debug.LogWarning("BgmProgressManager: BGM已在播放中!");
            return;
        }

        // 记录DSP开始时间（音频硬件同步，无帧延迟）
        DspStartTime = AudioSettings.dspTime;
        _songData.BgmAudioSource.PlayScheduled(DspStartTime);
        IsPlaying = true;
        Debug.Log($"BgmProgressManager: BGM开始播放,DSP时间={DspStartTime:F8}");
    }

    // 停止BGM播放
    public void StopBgmPlay()
    {
        if (_songData?.BgmAudioSource == null) return;

        IsPlaying = false;
        _songData.BgmAudioSource.Stop();
        PreciseTime = 0;
        Debug.Log("BgmProgressManager: BGM已停止播放");
    }

    // 更新精准播放进度（由主控高频调用）
    public void UpdatePreciseProgress()
    {
        if (!IsPlaying || _songData == null || _songData.BgmAudioSource == null)
        {
            PreciseTime = 0;
            return;
        }

        // 基于DSP时间计算精准进度（不受帧率/TimeScale影响）
        double currentDspTime = AudioSettings.dspTime;
        PreciseTime = (float)(currentDspTime - DspStartTime);
        // 限制进度不超过歌曲总时长
        PreciseTime = Mathf.Clamp(PreciseTime, 0, _songData.BgmClip.length);
    }

    // 检查是否播放完成
    public bool IsBgmFinished()
    {
        return _songData != null && _songData.BgmAudioSource != null 
               && !_songData.BgmAudioSource.isPlaying && IsPlaying;
    }
}