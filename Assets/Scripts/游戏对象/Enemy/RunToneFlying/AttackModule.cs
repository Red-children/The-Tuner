using UnityEngine;

/// <summary>
/// 攻击模块 - 处理敌人的攻击逻辑
/// </summary>
public class AttackModule
{
    private RunToneFlyingInsect owner;
    private PerceptionModule perception;
    private RunToneFlyingInsectDataManager dataManager;
    private float lastAttackTime;
    
    /// <summary>
    /// 初始化攻击模块
    /// </summary>
    /// <param name="owner">拥有者</param>
    /// <param name="perception">感知模块</param>
    public AttackModule(RunToneFlyingInsect owner, PerceptionModule perception)
    {
        this.owner = owner;
        this.perception = perception;
        this.dataManager = owner.GetComponent<RunToneFlyingInsectDataManager>();
        this.lastAttackTime = 0f;
    }
    
    /// <summary>
    /// 更新攻击状态
    /// </summary>
    public void Update()
    {
        if (CanAttack())
        {
            Attack();
        }
    }
    
    /// <summary>
    /// 检查是否可以攻击
    /// </summary>
    /// <returns>是否可以攻击</returns>
    private bool CanAttack()
    {
        return perception.CanSeePlayer && 
               perception.DistanceToPlayer <= dataManager.AttackRange && 
               Time.time >= lastAttackTime + dataManager.AttackCooldown;
    }
    
    /// <summary>
    /// 执行攻击
    /// </summary>
    private void Attack()
    {
        // 记录攻击时间
        lastAttackTime = Time.time;
        
        // 找到玩家对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 检查玩家是否在攻击范围内
            float distanceToPlayer = Vector3.Distance(owner.transform.position, player.transform.position);
            if (distanceToPlayer <= dataManager.AttackRange)
            {
                // 对玩家造成伤害
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(Mathf.RoundToInt(dataManager.BaseAttack));
                    Debug.Log("RunToneFlyingInsect attacks player for " + dataManager.BaseAttack + " damage");
                }
                else
                {
                    Debug.LogWarning("PlayerHealth component not found on player");
                }
            }
        }
        
        // 可以添加攻击动画或特效
    }
}
