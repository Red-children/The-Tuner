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

public struct PenetrationActivatedEvent { }

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
    public static ComboManager Instance { get; private set; }   //单例实例

    [Header("连击配置")]
    [SerializeField] private float comboTimeout = 3f;           // 连击超时时间
    [SerializeField] private int penetrationThreshold = 10;     // 穿透效果阈值
    [SerializeField] private int invincibilityThreshold = 15;   // 无敌效果阈值
    [SerializeField] private float invincibilityDuration = 3f;  // 无敌持续时间
    
    [Header("当前状态")]
    [SerializeField] private int currentCombo = 0;      //当前的连击数
    [SerializeField] private float lastHitTime = 0f;    //上一次命中的时间
    [SerializeField] private bool isComboActive = false;//combo系统是否激活
    
    private Coroutine comboTimeoutCoroutine;        //连击超时协程（注意这里存的只是引用）

    // 只读属性供外部访问
    public int CurrentCombo => currentCombo;    
    public bool IsComboActive => isComboActive;
    public float TimeSinceLastHit => Time.time - lastHitTime;
    
 
    private void Awake()
    {
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        // 取消事件监听
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
        }
    }

    #region 回调函数
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
        // 先获取当前节奏判定等级
        RhythmRank currentRank = RhythmManager.Instance.GetRank().rank;
        
        // 如果是Miss判定，立即中断连击
        if(currentRank == RhythmRank.Miss) 
        {
            BreakCombo();
            return;
        }

        // 其他判定等级：正常增加连击数
        currentCombo++;             //增加连击计数器
        lastHitTime = Time.time;    //记录当前时间
        isComboActive = true;       //标记连击系统为激活状态

        // 获取当前连击效果
        ComboEffect[] effects = GetCurrentComboEffects();
        var comboData = new ComboData(currentCombo, effects);
        
        // 触发连击更新事件
        EventBus.Instance.Trigger(new ComboChangedEvent(currentRank, currentCombo, comboTimeout));
        
        // 应用连击效果
        ApplyComboEffects(comboData);
        
        // 重置超时计时器
        ResetComboTimeout();
        
        Debug.Log($"连击数: {currentCombo}, 判定: {currentRank}, 效果: {string.Join(", ", effects)}");
    }
    #endregion

    /// <summary>
    /// 获取当前连击效果
    /// </summary>
    private ComboEffect[] GetCurrentComboEffects()
    {
        // 根据当前连击数判断应该给予哪些效果 
        System.Collections.Generic.List<ComboEffect> effects = new System.Collections.Generic.List<ComboEffect>();

        //判断连击数是否达到穿透效果阈值
        if (currentCombo >= penetrationThreshold)
        {
            //添加穿透效果
            effects.Add(ComboEffect.BulletPenetration);
        }

        //判断连击数是否达到无敌效果阈值
        if (currentCombo >= invincibilityThreshold)
        {
            //添加无敌效果
            effects.Add(ComboEffect.Invincibility);
        }

        //把效果列表转换为数组返回
        return effects.ToArray();
    }

    
    /// <summary>
    /// 应用连击效果
    /// </summary>
    private void ApplyComboEffects(ComboData comboData)
    {
        if (!comboData.HasEffects) return;

        // 根据连击效果执行相应的逻辑
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
            // 如果已经有一个超时协程在运行，先停止它
            StopCoroutine(comboTimeoutCoroutine);
        }
        //开启一个新的连击超时协程
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
      
        EventBus.Instance.Trigger(new ComboBreakEvent(currentRank, finalCombo, comboTimeout));
        
        // 触发连击更新事件（显示0连击）
        var comboData = new ComboData(0, Array.Empty<ComboEffect>());
       
        // EventBus.Instance.Trigger(new ComboChangedEvent(currentRank, finalCombo, comboTimeout));
        
        Debug.Log($"连击中断，最终连击数: {finalCombo}");
    }
    
    /// <summary>
    /// 手动重置连击（用于关卡切换等）
    /// </summary>
    public void ResetCombo()
    {
        currentCombo = 0;               // 重置连击计数器
        isComboActive = false;          // 标记连击系统为非激活状态

        if (comboTimeoutCoroutine != null)
        {
            StopCoroutine(comboTimeoutCoroutine);// 停止正在运行的连击超时协程
            comboTimeoutCoroutine = null;        // 清除协程引用   
        }

        //触发连击更新事件 并传递0连击和空效果
        var comboData = new ComboData(0, Array.Empty<ComboEffect>());
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