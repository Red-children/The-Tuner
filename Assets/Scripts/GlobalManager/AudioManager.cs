using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 音效管理器
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource sfxSource; // 用于播放一次性音效的公共AudioSource

    [Header("音效资源")]
    /// <summary>
    /// 玩家射击音效
    /// </summary>
    public AudioClip shootClip;
    /// <summary>
    /// 敌人击中音效
    /// </summary>
    public AudioClip hitClip;
    /// <summary>
    /// 敌人死亡音效
    /// </summary>
    public AudioClip enemyDeathClip;
    /// <summary>
    /// 敌人预警音效
    /// </summary>
    public AudioClip warningWarningClip;

    private float masterVolume = 1f;
    private float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 注册音量变化回调
        SettingsManager.Instance.RegisterCallback(SettingType.MasterVolume, OnVolumeChanged);
        SettingsManager.Instance.RegisterCallback(SettingType.SFXVolume, OnVolumeChanged);
        
        // 初始化音量
        UpdateVolumeSettings();
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<PlayerFiredEvent>(OnPlayerFired);
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
        EventBus.Instance.Subscribe<EnemyWarningEvent>(OnEnemyWarning);
        EventBus.Instance.Subscribe<EnemyDiedStruct>(OnEnemyDeath);
        
        // ... 订阅更多事件
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<PlayerFiredEvent>(OnPlayerFired);
        EventBus.Instance.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
        EventBus.Instance.Unsubscribe<EnemyWarningEvent>(OnEnemyWarning);
        EventBus.Instance.Unsubscribe<EnemyDiedStruct>(OnEnemyDeath);
        
        // 取消注册回调
        SettingsManager.Instance.UnregisterCallback(SettingType.MasterVolume, OnVolumeChanged);
        SettingsManager.Instance.UnregisterCallback(SettingType.SFXVolume, OnVolumeChanged);
    }

    private void OnVolumeChanged()
    {
        UpdateVolumeSettings();
    }

    private void UpdateVolumeSettings()
    {
        // 获取设置中的音量值（0-100）并转换为0-1范围
        masterVolume = SettingsManager.Instance.GetValue(SettingType.MasterVolume) / 100f;
        sfxVolume = SettingsManager.Instance.GetValue(SettingType.SFXVolume) / 100f;
        
        Debug.Log($"音量设置更新: 主音量={masterVolume}, 音效音量={sfxVolume}");
    }

    private void OnPlayerFired(PlayerFiredEvent evt)
    {
        if (sfxSource && shootClip)
            sfxSource.PlayOneShot(shootClip, GetFinalVolume(0.8f)); // 应用音量设置
    }

    private void OnEnemyHit(EnemyHitEvent evt)
    {
        if (sfxSource && hitClip)
            sfxSource.PlayOneShot(hitClip, GetFinalVolume(1f));
    }

    private void OnEnemyWarning(EnemyWarningEvent evt)
    {
        if (sfxSource && warningWarningClip)
            sfxSource.PlayOneShot(warningWarningClip, GetFinalVolume(1f));
    }

    private void OnEnemyDeath(EnemyDiedStruct evt)
    {
        if (sfxSource && enemyDeathClip)
        {
            Debug.Log($"敌人{evt.enemy.name} 死亡");}
            sfxSource.PlayOneShot(enemyDeathClip, GetFinalVolume(1f));
    }

    /// <summary>
    /// 计算最终音量（主音量 × 音效音量 × 自定义缩放）
    /// </summary>
    private float GetFinalVolume(float customScale = 1f)
    {
        return masterVolume * sfxVolume * customScale;
    }
}