using UnityEngine;

/// <summary>
/// 移动模块 - 处理敌人的非攻击状态移动逻辑
/// </summary>
public class RunToneFlyingMovementModule
{
    private RunToneFlyingInsect owner;
    private RunToneFlyingPerceptionModule perception;
    private RunToneFlyingInsectDataManager dataManager;
    private RunToneFlyingAttackModule attackModule;
    
    /// <summary>
    /// 初始化移动模块
    /// </summary>
    /// <param name="owner">拥有者</param>
    /// <param name="perception">感知模块</param>
    public RunToneFlyingMovementModule(RunToneFlyingInsect owner, RunToneFlyingPerceptionModule perception)
    {
        this.owner = owner;
        this.perception = perception;
        this.dataManager = owner.GetComponent<RunToneFlyingInsectDataManager>();
    }
    
    /// <summary>
    /// 设置攻击模块引用
    /// </summary>
    /// <param name="attackModule">攻击模块</param>
    public void SetAttackModule(RunToneFlyingAttackModule attackModule)
    {
        this.attackModule = attackModule;
    }
    
    /// <summary>
    /// 更新移动状态
    /// </summary>
    public void Update()
    {
        // 如果正在攻击，移动模块不处理移动
        if (attackModule != null && attackModule.IsAttacking)
        {
            return;
        }
        
        if (perception.CanSeePlayer)
        {
            ApproachPlayer();
        }
        else
        {
            Idle();
        }
    }
    
    /// <summary>
    /// 接近玩家（准备冲刺）
    /// </summary>
    private void ApproachPlayer()
    {
        // 保持理想攻击距离，准备冲刺
        float idealDistance = dataManager.DetectionRange * 0.6f;
        
        if (perception.DistanceToPlayer > idealDistance)
        {
            // 向玩家移动
            Vector2 direction = (perception.PlayerPosition - owner.transform.position).normalized;
            float adjustedSpeed = dataManager.MoveSpeed * GetRhythmMultiplier();
            
            owner.transform.position += new Vector3(direction.x * adjustedSpeed * Time.deltaTime, 
                                                 direction.y * adjustedSpeed * Time.deltaTime, 0);
        }
        else if (perception.DistanceToPlayer < idealDistance * 0.4f)
        {
            // 稍微后退，保持距离
            Vector2 direction = (perception.PlayerPosition - owner.transform.position).normalized;
            Vector2 awayDirection = -direction;
            float awaySpeed = dataManager.MoveSpeed * 0.3f;
            
            owner.transform.position += new Vector3(awayDirection.x * awaySpeed * Time.deltaTime, 
                                                 awayDirection.y * awaySpeed * Time.deltaTime, 0);
        }
        
        // 翻转精灵朝向玩家
        Vector3 toPlayer = perception.PlayerPosition - owner.transform.position;
        if (toPlayer.x < 0)
        {
            owner.spriteRenderer.flipX = true;
        }
        else
        {
            owner.spriteRenderer.flipX = false;
        }
    }
    
    /// <summary>
    ///  idle行为
    /// </summary>
    private void Idle()
    {
        // 简单的 idle 移动
        owner.transform.position += new Vector3(Mathf.Sin(Time.time) * dataManager.IdleMovementSpeed, 
                                             Mathf.Cos(Time.time) * dataManager.IdleMovementSpeed, 0);
    }
    
    /// <summary>
    /// 获取节奏倍率
    /// </summary>
    /// <returns>节奏倍率</returns>
    private float GetRhythmMultiplier()
    {
        // 使用节奏工具类获取倍率
        return EnemyRhythmProcessor.GetMoveSpeedMultiplier();
    }
}
