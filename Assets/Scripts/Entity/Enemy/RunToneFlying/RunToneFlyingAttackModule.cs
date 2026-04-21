using System.Collections;
using UnityEngine;

/// <summary>
/// 攻击模块 - 处理敌人的冲刺攻击逻辑
/// </summary>
public class RunToneFlyingAttackModule
{
    private RunToneFlyingInsect owner;
    private RunToneFlyingPerceptionModule perception;
    private RunToneFlyingInsectDataManager dataManager;
    
    // 攻击状态
    private float lastAttackTime;
    private bool isAttacking = false;
    private Coroutine chargeCoroutine;
    
    /// <summary>
    /// 初始化攻击模块
    /// </summary>
    /// <param name="owner">拥有者</param>
    /// <param name="perception">感知模块</param>
    public RunToneFlyingAttackModule(RunToneFlyingInsect owner, RunToneFlyingPerceptionModule perception)
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
        if (!isAttacking && CanAttack())
        {
            StartChargeAttack();
        }
    }
    
    /// <summary>
    /// 检查是否可以攻击
    /// </summary>
    /// <returns>是否可以攻击</returns>
    private bool CanAttack()
    {
        return perception.CanSeePlayer && 
               perception.DistanceToPlayer <= dataManager.DetectionRange && 
               Time.time >= lastAttackTime + dataManager.AttackCooldown;
    }
    
    /// <summary>
    /// 开始冲刺攻击
    /// </summary>
    private void StartChargeAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // 开始冲刺协程
        if (chargeCoroutine != null)
        {
            owner.StopCoroutine(chargeCoroutine);
        }
        chargeCoroutine = owner.StartCoroutine(ChargeAttackCoroutine());
        
        Debug.Log("RunToneFlyingInsect starts charge attack!");
    }
    
    /// <summary>
    /// 冲刺攻击协程
    /// </summary>
    private IEnumerator ChargeAttackCoroutine()
    {
        // 1. 准备阶段（短暂停顿，给玩家反应时间）
        yield return new WaitForSeconds(0.2f);
        
        // 2. 冲刺阶段
        Vector3 chargeDirection = (perception.PlayerPosition - owner.transform.position).normalized;
        float chargeDistance = Mathf.Min(perception.DistanceToPlayer, dataManager.DetectionRange);
        float chargeSpeed = dataManager.MoveSpeed * 2f; // 冲刺速度加倍
        
        // 冲刺位移
        float chargeTime = chargeDistance / chargeSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < chargeTime)
        {
            // 检测路径上的玩家
            CheckChargeHit(chargeDirection, chargeSpeed * Time.deltaTime);
            
            // 执行位移
            owner.transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            
            yield return null;
        }
        
        // 3. 结束攻击
        EndChargeAttack();
    }
    
    /// <summary>
    /// 检测冲刺路径上的玩家
    /// </summary>
    private void CheckChargeHit(Vector3 direction, float distance)
    {
        // 使用射线检测路径上的玩家
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            owner.transform.position,
            direction,
            distance,
            LayerMask.GetMask("Player")
        );
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // 计算伤害（距离越近伤害越高）
                    float damageMultiplier = Mathf.Clamp01(1f - (hit.distance / distance));
                    int finalDamage = Mathf.RoundToInt(dataManager.BaseAttack * (1f + damageMultiplier));
                    
                    playerHealth.TakeDamage(finalDamage);
                    Debug.Log("RunToneFlyingInsect hits player for " + finalDamage + " damage");
                    
                    // 只攻击第一个命中的玩家
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// 结束冲刺攻击
    /// </summary>
    private void EndChargeAttack()
    {
        isAttacking = false;
        chargeCoroutine = null;
        
        Debug.Log("RunToneFlyingInsect ends charge attack");
    }
    
    /// <summary>
    /// 是否正在攻击
    /// </summary>
    public bool IsAttacking => isAttacking;
}
