using UnityEngine;

/// <summary>
/// 跑调飞虫数据管理器 - 管理跑调飞虫的所有数据
/// </summary>
public class RunToneFlyingInsectDataManager : MonoBehaviour
{
    [Header("数据配置")]
    public RunToneFlyingInsectData insectData; // 跑调飞虫数据
    
    [Header("基础属性")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float idleMovementSpeed = 0.01f;
    
    [Header("特效预制体")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private GameObject deathEffectPrefab;
    
    // 只读属性
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float BaseAttack => baseAttack;
    public float MoveSpeed => moveSpeed;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float IdleMovementSpeed => idleMovementSpeed;
    public GameObject DamageTextPrefab => damageTextPrefab;
    public GameObject DeathEffectPrefab => deathEffectPrefab;
    
    private void Awake()
    {
        // 从ScriptableObject初始化数据
        if (insectData != null)
        {
            InitializeFromInsectData(insectData);
        }
        else
        {
            // 如果没有设置ScriptableObject，使用默认值
            currentHealth = maxHealth;
        }
    }
    
    /// <summary>
    /// 从RunToneFlyingInsectData初始化数据
    /// </summary>
    /// <param name="data">跑调飞虫数据</param>
    private void InitializeFromInsectData(RunToneFlyingInsectData data)
    {
        maxHealth = data.health;
        currentHealth = data.health;
        baseAttack = data.attackDamage;
        moveSpeed = data.moveSpeed;
        detectionRange = data.detectionRange;
        damageTextPrefab = data.damageTextPrefab;
        deathEffectPrefab = data.deadEff;
    }
    
    /// <summary>
    /// 修改血量
    /// </summary>
    /// <param name="delta">变化值</param>
    public void ModifyHealth(float delta)
    {
        float oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + delta, 0, maxHealth);
        
        if (Mathf.Abs(oldHealth - currentHealth) > 0.001f)
        {
            // 触发血量变化事件
            // 这里可以添加事件触发
            if (currentHealth <= 0)
            {
                // 触发死亡事件
                // 这里可以添加事件触发
            }
        }
    }
    
    /// <summary>
    /// 设置最大血量
    /// </summary>
    /// <param name="health">最大血量</param>
    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        currentHealth = health;
    }
    
    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="speed">移动速度</param>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    /// <summary>
    /// 设置检测范围
    /// </summary>
    /// <param name="range">检测范围</param>
    public void SetDetectionRange(float range)
    {
        detectionRange = range;
    }
    
    /// <summary>
    /// 设置攻击范围
    /// </summary>
    /// <param name="range">攻击范围</param>
    public void SetAttackRange(float range)
    {
        attackRange = range;
    }
    
    /// <summary>
    /// 设置攻击冷却时间
    /// </summary>
    /// <param name="cooldown">攻击冷却时间</param>
    public void SetAttackCooldown(float cooldown)
    {
        attackCooldown = cooldown;
    }
    
    /// <summary>
    /// 设置伤害飘字预制体
    /// </summary>
    /// <param name="prefab">伤害飘字预制体</param>
    public void SetDamageTextPrefab(GameObject prefab)
    {
        damageTextPrefab = prefab;
    }
    
    /// <summary>
    /// 设置死亡特效预制体
    /// </summary>
    /// <param name="prefab">死亡特效预制体</param>
    public void SetDeathEffectPrefab(GameObject prefab)
    {
        deathEffectPrefab = prefab;
    }
}
