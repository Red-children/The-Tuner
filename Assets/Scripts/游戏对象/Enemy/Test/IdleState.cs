using System.Data.Common;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

#region 等待状态

public class EnemyIdleState : IState
{
    private FSM manager;
    private Parameter parameter;
    // 等待计时器
    private float timer;

    public EnemyIdleState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入Idle状态");
        timer = 0f;
    }

    public void OnUpdate()
    {
        // 如果受到攻击，立即切换到受击状态
        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        timer += Time.deltaTime;
        // 如果等待时间超过设定值，切换到巡逻状态
        if (timer >= parameter.idleTime)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }
        // 如果在等待期间发现玩家，立即切换到追逐状态
        if (parameter.target != null)
        {
            manager.ChangeState(StateType.Chase);
        }
    }

    public void OnExit()
    {
        timer = 0f;
    }
}
#endregion

#region 巡逻状态
public class EnemyPatrolState : IState
{
    private FSM manager;             
    private Parameter parameter;
    private Vector2 targetPos;
    private float minDistance = 0.1f;



    public EnemyPatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    private int currentPointIndex = 0;
    private bool movingForward = true; // 用于往返

    public void OnStart()
    {
        Debug.Log("进入Patrol状态");
        if (parameter.patrolPoints == null || parameter.patrolPoints.Length == 0)
        {
            Debug.LogWarning("没有设置巡逻点，使用随机巡逻");
            GetNewRandomTarget(); // 保底逻辑
        }
        else
        {
            SetNextTarget();
        }
    }

    public void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        if (parameter.target != null)
        {
            manager.ChangeState(StateType.Chase);
            return;
        }



        // 根据目标点方向设置 flipX
        if (targetPos.x > manager.transform.position.x)
            parameter.spriteRenderer.flipX = false; // 目标在右，不翻转
        else if (targetPos.x < manager.transform.position.x)
            parameter.spriteRenderer.flipX = true;  // 目标在左，翻转

        // 向目标点移动
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            targetPos,
            parameter.moveSpeed * Time.deltaTime);

        // 到达目标点后获取下一个点
        if (Vector2.Distance(manager.transform.position, targetPos) < minDistance)
        {
            if (parameter.patrolPoints.Length > 0)
                SetNextTarget();
            else
                GetNewRandomTarget();
        }
    }

    #region 巡逻点切换方法
    private void SetNextTarget()
    {
        if (parameter.patrolPoints.Length == 0) return;
        targetPos = parameter.patrolPoints[currentPointIndex].position;

        // 更新索引（往返模式）
        if (movingForward)
        {
            if (currentPointIndex == parameter.patrolPoints.Length - 1)
                movingForward = false;
            else
                currentPointIndex++;
        }
        else
        {
            if (currentPointIndex == 0)
                movingForward = true;
            else
                currentPointIndex--;
        }
    }
    #endregion
    public void OnExit()
    {
        targetPos = Vector2.zero;
    }

    #region 旋转方法（暂时弃用）
    //private void RotateTowardsTarget()
    //{
    //    //目标点与当前敌人位置的方向向量
    //    Vector2 direction = targetPos - (Vector2)manager.transform.position;
    //    if (direction.magnitude < 0.01f) return;

    //    // 计算目标角度
    //    float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    //    // 获取当前角度（将四元数转换为欧拉角，注意我们只需要Z轴旋转）
    //    float currentAngle = manager.transform.eulerAngles.z;

    //    // 计算最短旋转角度差
    //    float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

    //    // 根据旋转速度限制每帧最大旋转角度
    //    float maxDelta = rotationSpeed * Time.deltaTime;
    //    float newAngle = currentAngle + Mathf.Clamp(angleDiff, -maxDelta, maxDelta);

    //    // 应用新旋转
    //    manager.transform.rotation = Quaternion.Euler(0, 0, newAngle);
    //}



    #endregion

    #region 随机巡逻点生成方法
    private void GetNewRandomTarget()
    {
        if (parameter.patrolCenter == null)
        {
            Debug.LogWarning("巡逻中心点未设置！");
            return;
        }

        // 优化随机点生成：偏向当前朝向的前方区域，减少突然掉头
        Vector2 center = parameter.patrolCenter.position;

        // 获取当前敌人的朝向（单位向量）
        Vector2 currentDir = manager.transform.right; // 假设敌人默认向右

        // 在朝向 ±90 度范围内随机角度，避免直接生成后方点
        float angleRange = 90f; // 可调整，越大越可能转向后方
        float randomAngle = Random.Range(-angleRange, angleRange) * Mathf.Deg2Rad;
        Vector2 randomDir = new Vector2(
            Mathf.Cos(randomAngle) * currentDir.x - Mathf.Sin(randomAngle) * currentDir.y,
            Mathf.Sin(randomAngle) * currentDir.x + Mathf.Cos(randomAngle) * currentDir.y
        ).normalized;

        // 随机半径
        float radius = Random.Range(0f, parameter.patrolRadius);
        Vector2 offset = randomDir * radius;

        // 最终目标点 = 巡逻中心 + 偏移
        targetPos = center + offset;
    }
    #endregion
}
#endregion

#region 追逐状态

public class EnemyChaseState : IState
{
    private FSM manager;
    private Parameter parameter;

    public EnemyChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入Chase状态");
    }

    public void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null) { manager.ChangeState(StateType.Patrol); return; }

        // 根据玩家位置设置 flipX
        if (parameter.target.position.x > manager.transform.position.x)
            parameter.spriteRenderer.flipX = false;
        else
            parameter.spriteRenderer.flipX = true;

        // 向玩家移动
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            parameter.target.position,
            parameter.chaseSpeed * Time.deltaTime);

        // 检查是否进入攻击范围（区分近战和远程）
        if (parameter.enemyType == EnemyType.Ranged)
        {
            float distance = Vector2.Distance(manager.transform.position, parameter.target.position);
            if (distance <= parameter.attackRange)
            {
                manager.ChangeState(StateType.Approach);
            }
        }
        else // 近战
        {
            if (Physics2D.OverlapCircle(manager.GetAttackWorldPos(), parameter.attackRange, parameter.targetLayer))
            {
                manager.ChangeState(StateType.Approach);
            }
        }
    }




    public void OnExit() { }
}
#endregion

#region 敌人受伤方法

public class EnemyWoundState : IState
{
    private FSM manager;
    private Parameter parameter;
    private float timer;        // 受击硬直计时器

    public float finallyDamage;//最终伤害值 在Wound方法中计算并赋值

    public EnemyWoundState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        parameter.health -= finallyDamage; // 直接在这里扣血，确保状态切换时已经计算好最终伤害
        Debug.Log("进入Wound状态");
        parameter.getHit = false;
        manager.ShowDamageText(manager.transform.position, finallyDamage);
        timer = 0f;
    
    }

    public void OnUpdate()
    {
        if (parameter.health <= 0)
        {
            manager.ChangeState(StateType.Dead);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= 0.5f) // 受击硬直时间
        {
            if (parameter.target != null)
                manager.ChangeState(StateType.Chase);
            else
                manager.ChangeState(StateType.Patrol);
        }
    }

    public void OnExit() { }
}

#endregion

public class EnemyDeadState : IState
{
    private FSM manager;
    private Parameter parameter;

    public EnemyDeadState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("敌人死亡");
        manager.Dead();
    }

    public void OnUpdate() { }
    public void OnExit() { }
}

public class EnemyMeleeApproachState: IState
{
    private FSM manager;
    private Parameter parameter;
    private float approachTime = 2f;
    private float timer;
    private Vector2 currentDirection;
    private float directionChangeTimer;
    private float directionChangeInterval = 0.5f; // 每0.5秒才考虑变向

    // 转向限制
    private float maxTurnAnglePerSec = 120f; // 每秒最多转120度

    public EnemyMeleeApproachState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入Approach状态");
        timer = 0f;
        parameter.animator.Play("Attack");
        // 初始方向朝向玩家
        if (parameter.target != null)
        {
            currentDirection = (parameter.target.position - manager.transform.position).normalized;
        }
        directionChangeTimer = directionChangeInterval;
    }

    public void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null) { manager.ChangeState(StateType.Patrol); return; }

        // 1. 计算期望方向（朝向玩家）
        Vector2 toPlayer = (parameter.target.position - manager.transform.position).normalized;

        // 2. 射线检测避障（简单版）
        RaycastHit2D hit = Physics2D.Raycast(manager.transform.position, currentDirection, 1.5f, LayerMask.GetMask("Wall"));
        if (hit.collider != null)
        {
            // 如果前方有墙，强制转向（比如向左转）
            toPlayer = Quaternion.Euler(0, 0, 45) * toPlayer;
        }

        // 3. 转向限制：不能直接从当前方向突变到期望方向
        float angleBetween = Vector2.SignedAngle(currentDirection, toPlayer);
        float maxDelta = maxTurnAnglePerSec * Time.deltaTime;
        float newAngle = Mathf.MoveTowardsAngle(
            Vector2.SignedAngle(Vector2.right, currentDirection),
            Vector2.SignedAngle(Vector2.right, toPlayer),
            maxDelta
        );
        currentDirection = Quaternion.Euler(0, 0, newAngle) * Vector2.right;

        // 4. 移动
        manager.transform.position += (Vector3)currentDirection * parameter.moveSpeed * Time.deltaTime;

        // 5. 面朝玩家（翻转）
        if (parameter.target.position.x > manager.transform.position.x)
            parameter.spriteRenderer.flipX = false;
        else
            parameter.spriteRenderer.flipX = true;

        // 6. 计时切换
        timer += Time.deltaTime;
        if (timer >= approachTime)
        {
            manager.ChangeState(StateType.Attack);
        }
    }

    public void OnExit() { }
}


public class EnemyMeleeAttackState : IState
{
    private FSM manager;
    private Parameter parameter;
    private float attackTimer;

    public EnemyMeleeAttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入近战攻击状态");
        attackTimer = 0f;
        parameter.animator.SetTrigger("Attack");
    }

    public void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null || !IsTargetInRange())
        {
            manager.ChangeState(StateType.Chase);
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f) // 攻击间隔，可从配置读取
        {
            attackTimer = 0f;
            // 实际伤害在动画事件中触发，这里只做冷却
        }
    }

    public void OnExit() { }

    private bool IsTargetInRange()
    {
        if (parameter.target == null) return false;
        Vector2 attackWorldPos = manager.GetAttackWorldPos();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackWorldPos, parameter.attackRange, parameter.targetLayer);
        foreach (var hit in hits)
            if (hit.transform == parameter.target) return true;
        return false;
    }

    // 动画事件调用
    public void OnAttackHit()
    {
        if (!IsTargetInRange()) return;
        PlayerIObject player = parameter.target.GetComponent<PlayerIObject>();
        if (player != null)
            player.Wound(parameter.attackDamage);
    }
}

public class EnemyRangedAttackState : IState
{
    private FSM manager;
    private Parameter parameter;

    public EnemyRangedAttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入远程攻击状态");
        parameter.animator.SetTrigger("Attack");
    }

    public void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null) { manager.ChangeState(StateType.Patrol); return; }

        float distance = Vector2.Distance(manager.transform.position, parameter.target.position);
        float attackRange = parameter.attackRange;

        // 如果太远或太近，回到接近状态
        if (distance > attackRange || distance < attackRange * 0.6f)
        {
            manager.ChangeState(StateType.Approach);
            return;
        }

        // 在有效范围内，使用武器射击
        if (parameter.rangedWeapon != null)
        {
            parameter.rangedWeapon.Shoot(); // 武器内部处理冷却
        }
    }

    public void OnExit() { }

    // 动画事件调用（如果需要）
    public void OnAttackHit() { }
}
public class EnemyRangedApproachState : IState
{
    private FSM manager;
    private Parameter parameter;
    private Vector2 currentDirection;

    public EnemyRangedApproachState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入远程接近状态");
        if (parameter.target != null)
            currentDirection = (parameter.target.position - manager.transform.position).normalized;
    }

    public void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null) { manager.ChangeState(StateType.Patrol); return; }

        float distance = Vector2.Distance(manager.transform.position, parameter.target.position);
        float attackRange = parameter.attackRange;

        // 定义理想距离范围：攻击范围的 60%~90%
        float minDesired = attackRange * 0.6f;
        float maxDesired = attackRange * 0.9f;

        Vector2 toTarget = (parameter.target.position - manager.transform.position).normalized;

        // 判断距离是否合适
        if (distance < minDesired)
        {
            // 太近，远离玩家
            currentDirection = -toTarget;
        }
        else if (distance > maxDesired)
        {
            // 太远，靠近玩家
            currentDirection = toTarget;
        }
        else
        {
            // 距离合适，立即切换到攻击状态
            manager.ChangeState(StateType.Attack);
            return;
        }

        // 执行移动
        manager.transform.position += (Vector3)currentDirection * parameter.moveSpeed * Time.deltaTime;

        // 面朝玩家（翻转）
        if (parameter.target.position.x > manager.transform.position.x)
            parameter.spriteRenderer.flipX = false;
        else
            parameter.spriteRenderer.flipX = true;
    }

    public void OnExit() { }
}