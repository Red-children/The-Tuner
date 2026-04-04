using System;
using System.Collections;
using UnityEngine;

#region 连击效果枚举
public enum ComboEffect
{
    None = 0,
    BulletPenetration = 1,    // 子弹穿透
    Invincibility = 2,        // 无敌状态
    DamageBoost = 3,          // 伤害提升
    SpeedBoost = 4            // 速度提升
}
#endregion

#region 连击数据结构
public struct ComboData
{
    public int CurrentCombo { get; private set; }
    public ComboEffect[] Effects { get; private set; }
    public bool HasEffects => Effects != null && Effects.Length > 0;
    
    public ComboData(int currentCombo, ComboEffect[] effects)
    {
        CurrentCombo = currentCombo;
        Effects = effects;
    }
}
#endregion

#region 连击事件
public struct ComboChangedEvent
{
    public RhythmRank rank;
    public int newCombo;
    public float comboTimeout;
    
    public ComboChangedEvent(RhythmRank rank, int newCombo, float comboTimeout)
    {
        this.rank = rank;
        this.newCombo = newCombo;
        this.comboTimeout = comboTimeout;
    }
}

public struct ComboBreakEvent
{
    public RhythmRank rank;
    public int finalCombo;
    public float comboTimeout;
    
    public ComboBreakEvent(RhythmRank rank, int finalCombo, float comboTimeout)
    {
        this.rank = rank;
        this.finalCombo = finalCombo;
        this.comboTimeout = comboTimeout;
    }
}
#endregion

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }
    
    [Header("连击配置")]
    [SerializeField] private float comboTimeout = 3f;           // 连击超时时间
    [SerializeField] private int penetrationThreshold = 10;     // 穿透效果阈值
    [SerializeField] private int invincibilityThreshold = 15;   // 无敌效果阈值
    [SerializeField] private float invincibilityDuration = 3f;  // 无敌持续时间
    
    [Header("当前状态")]
    [SerializeField] private int currentCombo = 0;
    [SerializeField] private float lastHitTime = 0f;
    [SerializeField] private bool isComboActive = false;
    
    private Coroutine comboTimeoutCoroutine;
    
    // 只读属性供外部访问
    public int CurrentCombo => currentCombo;
    public bool IsComboActive => isComboActive;
    public float TimeSinceLastHit => Time.time - lastHitTime;
    
    // 单事件设计，减少事件数量
    public event Action<ComboData> OnComboUpdate;
    public event Action<ComboBreakEvent> OnComboBreak;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // 监听敌人被命中事件
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
    }
    
    private void OnDestroy()
    {
        // 取消事件监听
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
        }
    }
    
    /// <summary>
    /// 敌人被命中时的处理
    /// </summary>
    private void OnEnemyHit(EnemyHitEvent hitEvent)
    {
        AddCombo();
    }
    
    /// <summary>
    /// 添加连击数
    /// </summary>
    public void AddCombo()
    {
        currentCombo++;
        lastHitTime = Time.time;
        isComboActive = true;
        
        // 获取当前连击效果
        ComboEffect[] effects = GetCurrentComboEffects();
        var comboData = new ComboData(currentCombo, effects);
        
        // 触发连击更新事件
        OnComboUpdate?.Invoke(comboData);
        RhythmRank currentRank = RhythmManager.Instance.GetRank().rank;
        EventBus.Instance.Trigger(new ComboChangedEvent(currentRank, currentCombo, comboTimeout));
        
        // 应用连击效果
        ApplyComboEffects(comboData);
        
        // 重置超时计时器
        ResetComboTimeout();
        
        Debug.Log($"连击数: {currentCombo}, 效果: {string.Join(", ", effects)}");
    }
    
    /// <summary>
    /// 获取当前连击效果
    /// </summary>
    private ComboEffect[] GetCurrentComboEffects()
    {
        System.Collections.Generic.List<ComboEffect> effects = new System.Collections.Generic.List<ComboEffect>();
        
        if (currentCombo >= penetrationThreshold)
        {
            effects.Add(ComboEffect.BulletPenetration);
        }
        
        if (currentCombo >= invincibilityThreshold)
        {
            effects.Add(ComboEffect.Invincibility);
        }
        
        return effects.ToArray();
    }
    
    /// <summary>
    /// 应用连击效果
    /// </summary>
    private void ApplyComboEffects(ComboData comboData)
    {
        if (!comboData.HasEffects) return;
        
        foreach (var effect in comboData.Effects)
        {
            switch (effect)
            {
                case ComboEffect.BulletPenetration:
                    // 子弹穿透效果由子弹模块处理
                    break;
                    
                case ComboEffect.Invincibility:
                    // 无敌效果由玩家模块处理
                    PlayerManager.Instance?.EnableInvincibility(invincibilityDuration);
                    break;
            }
        }
    }
    
    /// <summary>
    /// 重置连击超时计时器
    /// </summary>
    private void ResetComboTimeout()
    {
        if (comboTimeoutCoroutine != null)
        {
            StopCoroutine(comboTimeoutCoroutine);
        }
        comboTimeoutCoroutine = StartCoroutine(ComboTimeoutCoroutine());
    }
    
    /// <summary>
    /// 连击超时协程
    /// </summary>
    private IEnumerator ComboTimeoutCoroutine()
    {
        yield return new WaitForSeconds(comboTimeout);
        
        // 超时后重置连击
        if (currentCombo > 0)
        {
            BreakCombo();
        }
    }
    
    /// <summary>
    /// 强制中断连击
    /// </summary>
    public void BreakCombo()
    {
        if (currentCombo == 0) return;
        
        int finalCombo = currentCombo;
        currentCombo = 0;
        isComboActive = false;
        RhythmRank currentRank = RhythmManager.Instance.GetRank().rank;
        // 触发连击中断事件
        OnComboBreak?.Invoke(new ComboBreakEvent(currentRank, finalCombo, comboTimeout));
        EventBus.Instance.Trigger(new ComboBreakEvent(currentRank, finalCombo, comboTimeout));
        
        // 触发连击更新事件（显示0连击）
        var comboData = new ComboData(0, Array.Empty<ComboEffect>());
        OnComboUpdate?.Invoke(comboData);
        // EventBus.Instance.Trigger(new ComboChangedEvent(currentRank, finalCombo, comboTimeout));
        
        Debug.Log($"连击中断，最终连击数: {finalCombo}");
    }
    
    /// <summary>
    /// 手动重置连击（用于关卡切换等）
    /// </summary>
    public void ResetCombo()
    {
        currentCombo = 0;
        isComboActive = false;
        
        if (comboTimeoutCoroutine != null)
        {
            StopCoroutine(comboTimeoutCoroutine);
            comboTimeoutCoroutine = null;
        }
        
        // 触发连击更新事件
        var comboData = new ComboData(0, Array.Empty<ComboEffect>());
        OnComboUpdate?.Invoke(comboData);
        RhythmRank currentRank = RhythmManager.Instance.GetRank().rank;
        EventBus.Instance.Trigger(new ComboChangedEvent(currentRank, 0, comboTimeout));
    }
    
    /// <summary>
    /// 检查是否具有特定效果
    /// </summary>
    public bool HasEffect(ComboEffect effect)
    {
        var effects = GetCurrentComboEffects();
        return System.Array.Exists(effects, e => e == effect);
    }
}