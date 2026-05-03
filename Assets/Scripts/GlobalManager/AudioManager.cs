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



    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
    }

    private void OnPlayerFired(PlayerFiredEvent evt)
    {
        if (sfxSource && shootClip)
            sfxSource.PlayOneShot(shootClip, 0.8f); // 播放射击音效，0.8是音量
    }

    private void OnEnemyHit(EnemyHitEvent evt)
    {
        if (sfxSource && hitClip)
            sfxSource.PlayOneShot(hitClip);
    }

    private void OnEnemyWarning(EnemyWarningEvent evt)
    {
        if (sfxSource && warningWarningClip)
            sfxSource.PlayOneShot(warningWarningClip);
    }

    private void OnEnemyDeath(EnemyDiedStruct evt)
    {
        if (sfxSource && enemyDeathClip)
        {
            Debug.Log($"敌人{evt.enemy.name} 死亡");}
            sfxSource.PlayOneShot(enemyDeathClip);
    }
}