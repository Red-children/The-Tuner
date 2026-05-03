using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(RectTransform), typeof(AudioSource))]
public class UISoundPlayer : MonoBehaviour
{
    [Header("音效配置")]
    [SerializeField] private AudioClip onOpenClip;    // 打开音效
    [SerializeField] private AudioClip onCloseClip;   // 关闭音效
    
    [Header("Audio Mixer(可选)")]
    [SerializeField] private AudioMixerGroup mixerGroup;
    
    [SerializeField] private AudioSource audioSource;
    private bool isOpen = false;
    private bool isInitialized = false;
    
    private void Awake()
    {
        
        if (audioSource == null)
        {
            Debug.LogError($"UISoundPlayer: 预制体 {gameObject.name} 缺少 AudioSource 组件", gameObject);
            return;
        }
        
        // 配置 AudioSource 播放参数
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D 音效
        
        // 设置 Mixer Group（如果配置了）
        if (mixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
        }
        
        // 注册音量变化回调
        RegisterVolumeCallback();
        isInitialized = true;
    }
    
    private void OnDestroy()
    {
        // 清理回调
        UnregisterVolumeCallback();
    }
    
    private void OnEnable()
    {
        if (!isOpen && isInitialized)
        {
            PlayOpenSound();
            isOpen = true;
        }
    }
    
    private void OnDisable()
    {
        if (isOpen)
        {
            PlayCloseSound();
            isOpen = false;
        }
    }
    
    private void RegisterVolumeCallback()
    {
        // 注册音效音量变化回调
        SettingsManager.Instance.RegisterCallback(SettingType.SFXVolume, OnVolumeChanged);
    }
    
    private void UnregisterVolumeCallback()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.UnregisterCallback(SettingType.SFXVolume, OnVolumeChanged);
        }
    }
    
    private void OnVolumeChanged()
    {
        // 音量变化时的处理（可选，如果需要实时改变正在播放的音效）
        // PlayOneShot 每次调用都会使用新音量，所以无需额外处理
    }
    
    private float GetCurrentVolume()
    {
        // 从 SettingsManager 获取当前音效音量（0-100 转换为 0-1）
        int volumePercent = SettingsManager.Instance.GetValue(SettingType.SFXVolume);
        if (volumePercent < 0) volumePercent = 70; // 默认 70%
        
        return volumePercent / 100f;
    }
    
    private void PlayOpenSound()
    {
        if (audioSource == null)
        {
            Debug.LogError("UISoundPlayer: AudioSource 为空");
            return;
        }
        
        if (onOpenClip == null)
        {
            Debug.LogWarning($"UISoundPlayer: 预制体 {gameObject.name} 未配置打开音效");
            return;
        }
        
        float currentVolume = GetCurrentVolume();
        audioSource.PlayOneShot(onOpenClip, currentVolume);
    }
    
    private void PlayCloseSound()
    {
        if (audioSource == null) return;
        if (onCloseClip == null) return;
        
        float currentVolume = GetCurrentVolume();
        audioSource.PlayOneShot(onCloseClip, currentVolume);
    }
    
    // 可选：提供公有方法供外部调用
    public void PlayOpenSoundManually()
    {
        PlayOpenSound();
    }
    
    public void PlayCloseSoundManually()
    {
        PlayCloseSound();
    }
}