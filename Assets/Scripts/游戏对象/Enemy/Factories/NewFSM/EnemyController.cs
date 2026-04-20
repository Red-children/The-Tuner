using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// 敌人控制器类，继承自EnemyBase，负责管理敌人的行为和状态，包括初始化数据、处理受伤和死亡逻辑、显示伤害飘字等，同时通过状态机（FSM）来控制敌人的不同状态和行为，使得敌人能够根据玩家的行为做出相应的反应，增强了游戏的互动性和挑战性。
/// </summary>
public class EnemyController : EnemyBase
{
    //网格地图导航智能体
    public NavMeshAgent agent;

    public bool isFacingRight = true; // 默认朝向，根据初始旋转自行调整

    [Header("敌人数据")]
    public EnemyData data;

    [Header("敌人组件")]
    public WeaponInfo weapon;
    public Collider2D chaseArea;            // 追逐范围碰撞体
    public Transform patrolCenter;          // 巡逻中心点
    public Transform[] patrolPoints;        // 巡逻点数组

    public Collider2D weaponCollider;        // 武器碰撞体，用于近战攻击的伤害判定

    private Vector2 currentForward; //目前正确的方向

    [Header("状态机组件")]
    // 运行时数据
    [SerializeField] public EnemyRuntime runtime; // 运行时数据，包含当前状态、目标等动态信息
    //状态机实例
    [SerializeField] private FSM fsm;

    [Header("攻击预警UI")]
    [SerializeField] private EnemyWarningUI warningUI; // 在Inspector中拖拽敌人头顶的Canvas

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (weapon == null) weapon = GetComponentInChildren<WeaponInfo>();

        if (warningUI == null)
            warningUI = GetComponentInChildren<EnemyWarningUI>(true);
        currentForward = isFacingRight ? Vector2.right : Vector2.left;

        // 初始化武器碰撞体伤害脚本
        if (weaponCollider != null)
        {
            var hitScript = weaponCollider.GetComponent<EnemyWeaponHit>();
            if (hitScript == null)
                hitScript = weaponCollider.gameObject.AddComponent<EnemyWeaponHit>();
            hitScript.owner = this;
            // 根据敌人数据类型获取伤害值
            if (data is MeleeEnemyData meleeData)
                hitScript.damage = meleeData.attackDamage;
            // 初始禁用
            weaponCollider.enabled = false;
        }

        if (weaponCollider == null)
        {
            // 优先从武器对象下找
            if (weapon != null)
                weaponCollider = weapon.GetComponentInChildren<Collider2D>();
            // 找不到则通过名称查找
            if (weaponCollider == null)
            {
                Transform weaponTrans = transform.Find("Weapon");
                if (weaponTrans != null)
                    weaponCollider = weaponTrans.GetComponent<Collider2D>();
            }
            if (weaponCollider == null)
                Debug.LogError($"[{gameObject.name}] 未找到武器碰撞体！");
        }


        // 初始化状态机和运行时数据
        if (Application.isPlaying)
        {
            // 防止重复添加
            runtime = GetComponent<EnemyRuntime>();
            if (runtime == null)
                runtime = gameObject.AddComponent<EnemyRuntime>();
            runtime.Init(data);//根据敌人数据初始化运行时数据

            fsm = GetComponent<FSM>();
            if (fsm == null)
                fsm = gameObject.AddComponent<FSM>();

            // 获取对应敌人类型的状态工厂
            IStateFactory factory = GetStateFactory();
            fsm.Initialize(runtime, factory, this);
        }


        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        // 关键配置：关闭自动旋转，2D游戏我们自己控制朝向
        agent.updateRotation = false;
        agent.updateUpAxis = false; // 防止在XY平面上自动调整Y轴
    }
    // 实现基类的抽象方法
    protected override void UpdateBehavior()
    {
        // EnemyController使用状态机，这里不需要额外的行为逻辑
        // 状态机的更新在FSM类中处理
    }

    /// <summary>
    /// 判断敌人类型并返回对应的状态工厂
    /// </summary>
    /// <returns></returns>
    private IStateFactory GetStateFactory()
    {
        if (data is MeleeEnemyData) return new MeleeStateFactory();
        if (data is RangedEnemyData) return new RangedStateFactory();
        if (data is RunToneFlyingInsectData) return new RunToneFlyingStateFactory();
        if (data is NoiseMonsterData) return new MeleeStateFactory(); // 基础状态仍用近战工厂

        //if (data is ExplosiveEnemyData) return new ExplosiveStateFactory();
        return null;
    }

    /// <summary>
    /// 接口 供子弹设置攻击者的位置 用于敌人击退逻辑
    /// </summary>
    /// <param name="pos"></param>
    public void SetAttackerPosition(Vector2 pos)
    {
        if (runtime != null)
            runtime.lastAttackerPosition = pos;
    }


    /// <summary>
    /// 敌人受伤 切换状态
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="rank"></param>
    public override void Wound(float damage, RhythmRank rank)
    {
        //测试打断逻辑区域 *******************************
        if (runtime.isVulnerable && (rank == RhythmRank.Perfect || rank == RhythmRank.Great))
        {
            ShowDamageText(transform.position, damage, rank);
            Debug.Log("嘶吼被打断！进入眩晕状态");

            fsm?.ChangeState(StateType.NoiseStun);
            return; // 直接进入眩晕，不执行后续扣血（可根据设计调整）
        }
        //测试打断逻辑区域 *******************************


        Debug.Log("当前是否可以被打断" + runtime.isVulnerable);
        if (runtime.getHit) return;
        runtime.getHit = true;
        runtime.currentHealth -= damage;



        // 计算击退（使用之前设置的位置）
        Vector2 knockbackDir = ((Vector2)transform.position - runtime.lastAttackerPosition).normalized;
        float knockbackDist = GetKnockbackDistance(rank);
        runtime.knockbackForce = knockbackDir * knockbackDist;
        runtime.knockbackDistance = knockbackDist;

        ShowDamageText(transform.position, damage, rank);
        fsm?.ChangeState(StateType.Wound);
    }

    /// <summary>
    /// 计算击退的倍率
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    private float GetKnockbackDistance(RhythmRank rank)
    {
        switch (rank)
        {
            case RhythmRank.Perfect: return 2.5f;
            case RhythmRank.Great: return 1.5f;
            case RhythmRank.Good: return 0.8f;
            default: return 0f;
        }
    }

    #region  敌人死亡逻辑
    // 死亡处理
    public void Dead()
    {
        StartCoroutine(DeadCoroutine());
    }

    /// <summary>
    /// 死亡协程，延迟销毁以确保受伤状态有足够时间处理
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeadCoroutine()
    {
        runtime.getHit = false;

        // 调用基类的OnKilled方法
        OnKilled();

        // 等待1秒，确保受伤状态有足够时间处理
        yield return null;

        Destroy(gameObject);
    }

    // 显示伤害飘字
    public override void ShowDamageText(Vector3 targetPosition, float damage, RhythmRank rank)
    {
        if (runtime?.DamageTextPrefab == null) return;
        GameObject dmgObj = Instantiate(runtime.DamageTextPrefab, targetPosition, Quaternion.identity);
        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
        dmgNumber?.SetDamage(damage, rank); // 改动点
    }



    /// <summary>
    /// 敌人死亡携程，因为需要等待一会让敌人死亡事件被正确的发出
    /// </summary>
    /// <returns></returns>
    public override IEnumerator DeathCoroutine()
    {
        // 标记为死亡状态
        isDead = true;

        // 调用OnKilled方法处理死亡逻辑
        OnKilled();

        // 等待一帧，确保死亡逻辑执行完成
        yield return null;

        // 销毁游戏对象
        Destroy(gameObject);
    }


    // 被杀死时调用
    public override void OnKilled()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} 已被杀死");

        // 触发敌人死亡事件（传递敌人信息）
        EventBus.Instance.Trigger(new EnemyDiedStruct(this, transform.position));

        // 注销敌人（添加null检查避免异常）
        if (ownerRoom != null)
        {
            ownerRoom.UnregisterEnemy(this);
            Debug.Log($"{gameObject.name} 已从房间注销");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 的ownerRoom为null，无法注销");
        }

        // 播放死亡特效（使用安全的属性访问器）
        //if (runtime?.DeadEff != null)
        //    Instantiate(runtime.DeadEff, transform.position, transform.rotation);
    }

    #endregion

    #region  检测玩家进入预警区域的回调函数

    /// <summary>
    /// 当玩家进入检测区域时的回调函数
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerEnter(Transform player)
    {
        Debug.Log("玩家进入警戒区域");
        if (runtime != null)
        {
            runtime.target = player;
            runtime.isPursuing = true;          // 新发现目标，立即激活追击
            runtime.ignoreTargetUntilTime = 0f; // 重置冷却
        }
    }

    /// <summary>
    /// 当玩家离开检测区域时的回调函数
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerExit(Transform player)
    {
        if (runtime != null && runtime.target == player)
        {
            runtime.target = null;
            runtime.isPursuing = false;
            runtime.ignoreTargetUntilTime = 0f;
        }
    }
    #endregion


    /// <summary>
    /// 得到攻击点的世界坐标，主要用于近战敌人进行攻击范围的检测和伤害判定，根据敌人数据中的攻击偏移量计算出攻击点的位置，确保敌人能够正确地判断何时可以攻击玩家，同时根据敌人朝向动态调整攻击点的位置，使得攻击范围能够正确地覆盖玩家所在的位置，增强了游戏的互动性和挑战性。
    /// </summary>
    /// <returns></returns>
    public Vector2 GetAttackWorldPos()
    {
        if (data is MeleeEnemyData meleeData)
        {
            Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
            Vector2 attackOffset = forward * meleeData.attackOffset.x + Vector2.up * meleeData.attackOffset.y;
            return (Vector2)transform.position + attackOffset;
        }
        return transform.position;
    }
    /// <summary>
    /// 更新武器朝向，主要用于远程敌人根据玩家的位置动态调整    
    /// <>/summary>    
    public void UpdateWeaponAim()
    {
        if (weapon != null && runtime.target != null)
        {
            Vector2 direction = runtime.target.position - weapon.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            weapon.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    #region  绘制函数

    /// <summary>
    /// 在编辑器中绘制攻击范围，主要用于调试和设计阶段，帮助开发者可视化敌人的攻击范围和攻击点位置，根据敌人数据中的攻击参数绘制出攻击范围的边界，同时根据敌人朝向动态调整攻击点的位置，使得开发者能够更直观地了解敌人的攻击范围和行为逻辑，增强了游戏的设计效率和质量。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        SpriteRenderer sr = spriteRenderer ? spriteRenderer : GetComponent<SpriteRenderer>();

        if (data is MeleeEnemyData meleeData)
        {
            Vector2 forward;
            if (Application.isPlaying)
            {
                forward = isFacingRight ? Vector2.right : Vector2.left;
            }
            else
            {
                // 编辑器模式：根据当前 Y 轴旋转判断
                bool faceRight = Mathf.Approximately(transform.eulerAngles.y, 180f);
                forward = faceRight ? Vector2.right : Vector2.left;
            }

            Vector2 attackPos = (Vector2)transform.position + forward * meleeData.attackOffset.x + Vector2.up * meleeData.attackOffset.y;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPos, meleeData.attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPos, 0.1f);
        }
        else if (data is RangedEnemyData rangedData)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, rangedData.attackRange);
        }

        DrawVisionCone();

    }


    /// <summary>
    /// 绘制视线锥
    /// </summary>
    private void DrawVisionCone()
    {
        if (data == null) return;
        float range = data.visionRange;
        float angle = data.visionAngle;
        Vector2 forward = currentForward;

        float halfAngle = angle * 0.5f;
        Vector2 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward;
        Vector2 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward;

        // 绘制边界线
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftBoundary * range);
        Gizmos.DrawRay(transform.position, rightBoundary * range);

        // 绘制圆弧（分段直线近似）
        int segments = 20;
        Vector3 prevPoint = transform.position + (Vector3)(rightBoundary * range);
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 dir = Quaternion.Euler(0, 0, currentAngle) * forward;
            Vector3 point = transform.position + dir * range;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
    #endregion


    /// <summary>
    /// 计算面向目标的转向角
    /// </summary>
    /// <param name="targetPosition"></param>
    public void FaceTarget(Vector2 targetPosition)
    {
        isFacingRight = targetPosition.x > transform.position.x;
        currentForward = isFacingRight ? Vector2.right : Vector2.left;//得到目前的前方
        // 保持原有旋转逻辑（为了图像翻转）
        Vector3 rotation = transform.eulerAngles;
        rotation.y = isFacingRight ? 180 : 0;
        transform.eulerAngles = rotation;
    }

    #region  动画回调函数
    public void OnAttackAnimationEvent()
    {
        if (weaponCollider == null)
        {
            Debug.LogWarning("尝试重新获取武器碰撞体...");
            weaponCollider = GetComponentInChildren<Collider2D>();
            if (weaponCollider == null) return;
            // 补充伤害脚本初始化...
        }
        weaponCollider.enabled = true;
        weaponCollider.GetComponent<EnemyWeaponHit>()?.ResetHitFlag();
    }

    public void OnAttackAnimationEventEnd()
    {
        if (weaponCollider != null)
            weaponCollider.enabled = false;
    }

    public void OnAttackFinished()
    {
        // 由动画最后一帧调用，结束攻击状态
        if (fsm != null)
            fsm.ChangeState(StateType.Chase);
    }
    #endregion

    /// <summary>
    /// 设置当前敌人攻击目标
    /// </summary>
    /// <param name="target"></param>
    public override void SetTarget(Transform target)
    {
        if (runtime != null)
            runtime.target = target;
    }
    /// <summary>
    /// 展示攻击预警UI
    /// </summary>
    public void ShowAttackWarning()
    {
        if (warningUI != null)
            warningUI.PlayWarning();
    }
    /// <summary>
    /// 检测敌人是否能看到玩家
    /// </summary>
    /// <returns></returns>
    public bool CanSeePlayer()
    {
        //目标为空不进行
        if (runtime.target == null) { Debug.Log("目标检测不通过"); return false; }
        //计算当前的朝向和敌人的位置之间的方向向量
        Vector2 toPlayer = runtime.target.position - transform.position;
        float distance = toPlayer.magnitude;        //得到向量长度
        //距离检测
        if (distance > data.visionRange) { Debug.Log("距离检测不通过"); return false; }

        Vector2 forward = currentForward;
        float angle = Vector2.Angle(forward, toPlayer);//计算转向角
        float halfAngle = data.visionAngle * 0.5f;//计算半角

        Debug.Log($"[{name}] 视线检测:\n" +
                  $"  前方方向(forward): {forward}\n" +
                  $"  指向玩家方向: {toPlayer.normalized}\n" +
                  $"  夹角: {angle:F2}°, 半角阈值: {halfAngle:F2}°");

        if (angle > halfAngle) { Debug.Log("角度检测不通过"); return false; }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, data.visionBlockMask);
        return hit.collider == null;
    }

  public void OnWoundEnd()
    {
        //取出敌人受伤类
        EnemyWoundState enemyWoundState = fsm.currentState as  EnemyWoundState;
        if(enemyWoundState!=null)
        {
            enemyWoundState.HandleAnimationFinished();
            enemyWoundState.isAnimationPlaying = false;
        }
    } 

}