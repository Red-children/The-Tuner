using UnityEngine;

// 歌曲进程管理模块（仅处理播放状态和进度计算）

/// <summary>
/// 全局的BGM进程管理器，负责跟踪当前BGM的播放状态和精准进度,挂载在PreciseBGMManager
/// </summary>
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
    private float _masterVolume = 1f;
    private float _musicVolume = 1f;

    // 初始化（由主控调用）
    public void Init(BgmSongData songData)
    {
        _songData = songData;
        IsPlaying = false;
        PreciseTime = 0;
        DspStartTime = 0;
        
        // 注册音量变化回调
        SettingsManager.Instance.RegisterCallback(SettingType.MasterVolume, OnVolumeChanged);
        SettingsManager.Instance.RegisterCallback(SettingType.SFXVolume, OnVolumeChanged);
        UpdateVolumeSettings();
    }

    private void OnDestroy()
    {
        // 取消注册回调
        SettingsManager.Instance.UnregisterCallback(SettingType.MasterVolume, OnVolumeChanged);
        SettingsManager.Instance.UnregisterCallback(SettingType.SFXVolume, OnVolumeChanged);
    }

    private void OnVolumeChanged()
    {
        UpdateVolumeSettings();
        ApplyVolumeToAudioSource();
    }

    private void UpdateVolumeSettings()
    {
        // 获取设置中的音量值（0-100）并转换为0-1范围
        _masterVolume = SettingsManager.Instance.GetValue(SettingType.MasterVolume) / 100f;
        _musicVolume = SettingsManager.Instance.GetValue(SettingType.SFXVolume) / 100f;
        
        Debug.Log($"BGM音量设置更新: 主音量={_masterVolume}, 音乐音量={_musicVolume}");
    }

    private void ApplyVolumeToAudioSource()
    {
        if (_songData?.BgmAudioSource != null)
        {
            float finalVolume = _masterVolume * _musicVolume;
            _songData.BgmAudioSource.volume = finalVolume;
            Debug.Log($"BGM音量应用: {finalVolume}");
        }
    }

    // 启动BGM播放（精准调度）
    public void StartBgmPlay()
    {
        if (_songData == null || _songData.BgmAudioSource == null || _songData.GetAudioClip() == null)
        {
            Debug.LogError("BgmProgressManager: 歌曲配置或音频源未初始化！");
            return;
        }

        if (_songData.BgmAudioSource.isPlaying)
        {
            Debug.LogWarning("BgmProgressManager: BGM已在播放中!");
            return;
        }

        // 应用当前音量设置
        ApplyVolumeToAudioSource();

        // 记录DSP开始时间（音频硬件同步，无帧延迟）
        DspStartTime = AudioSettings.dspTime;
        _songData.BgmAudioSource.PlayScheduled(DspStartTime);//在指定的DSP时间开始播放，确保精准同步
        IsPlaying = true;//标记正在播放
        Debug.Log($"BgmProgressManager: BGM开始播放,DSP时间={DspStartTime:F8}");
    }

    // 停止BGM播放
    public void StopBgmPlay()
    {
        if (_songData?.BgmAudioSource == null) return;

        IsPlaying = false;
        _songData.BgmAudioSource.Stop();
        PreciseTime = 0;//重置进度
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
        PreciseTime = Mathf.Clamp(PreciseTime, 0, _songData.GetAudioClip().length);
    }

    // 检查是否播放完成
    public bool IsBgmFinished()
    {
        return _songData != null && _songData.BgmAudioSource != null 
               && !_songData.BgmAudioSource.isPlaying && IsPlaying;
    }



    /// <summary> 手动标记为播放中（供外部在直接使用AudioSource.Play()后同步状态） </summary>
    public void MarkAsPlaying()
    {
        IsPlaying = true;
        DspStartTime = AudioSettings.dspTime;
    }
}