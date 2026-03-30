using UnityEngine;

/// <summary>
/// 移动模块 - 处理敌人的移动逻辑
/// </summary>
public class MovementModule
{
    private RunToneFlyingInsect owner;
    private PerceptionModule perception;
    private RunToneFlyingInsectDataManager dataManager;
    private float safeDistance = 0.5f; // 与玩家的安全距离
    
    /// <summary>
    /// 初始化移动模块
    /// </summary>
    /// <param name="owner">拥有者</param>
    /// <param name="perception">感知模块</param>
    public MovementModule(RunToneFlyingInsect owner, PerceptionModule perception)
    {
        this.owner = owner;
        this.perception = perception;
        this.dataManager = owner.GetComponent<RunToneFlyingInsectDataManager>();
    }
    
    /// <summary>
    /// 更新移动状态
    /// </summary>
    public void Update()
    {
        if (perception.CanSeePlayer)
        {
            ChasePlayer();
        }
        else
        {
            Idle();
        }
    }
    
    /// <summary>
    /// 追逐玩家
    /// </summary>
    private void ChasePlayer()
    {
        if (perception.DistanceToPlayer > safeDistance)
        {
            // 计算移动方向
            Vector2 direction = (perception.PlayerPosition - owner.transform.position).normalized;
            
            // 同步BPM移动
            float rhythmMultiplier = GetRhythmMultiplier();
            float adjustedSpeed = dataManager.MoveSpeed * rhythmMultiplier;
            
            // 移动
            owner.transform.position += new Vector3(direction.x * adjustedSpeed * Time.deltaTime, 
                                                 direction.y * adjustedSpeed * Time.deltaTime, 0);
            
            // 翻转精灵
            if (direction.x < 0)
            {
                owner.spriteRenderer.flipX = true;
            }
            else
            {
                owner.spriteRenderer.flipX = false;
            }
        }
        else
        {
            // 当距离过近时，稍微远离玩家
            Vector2 direction = (perception.PlayerPosition - owner.transform.position).normalized;
            Vector2 awayDirection = -direction;
            float awaySpeed = dataManager.MoveSpeed * 0.5f;
            owner.transform.position += new Vector3(awayDirection.x * awaySpeed * Time.deltaTime, 
                                                 awayDirection.y * awaySpeed * Time.deltaTime, 0);
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
    
    /// <summary>
    /// 设置安全距离
    /// </summary>
    /// <param name="distance">安全距离</param>
    public void SetSafeDistance(float distance)
    {
        safeDistance = distance;
    }
}
