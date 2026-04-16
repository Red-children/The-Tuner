using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// 敌人控制器类，继承自EnemyBase，负责管理敌人的行为和状态，包括初始化数据、处理受伤和死亡逻辑、显示伤害飘字等，同时通过状态机（FSM）来控制敌人的不同状态和行为，使得敌人能够根据玩家的行为做出相应的反应，增强了游戏的互动性和挑战性。
/// </summary>
public class EnemyController : EnemyBase
{
    public bool isFacingRight = true; // 默认朝向，根据初始旋转自行调整
    [Header("敌人数据")]
    public EnemyData data;

    [Header("敌人组件")]
    public WeaponInfo weapon;
    public Collider2D chaseArea;            // 追逐范围碰撞体
    public Transform patrolCenter;          // 巡逻中心点
    public Transform[] patrolPoints;        // 巡逻点数组

    public Collider2D weaponCollider;        // 武器碰撞体，用于近战攻击的伤害判定

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


        print(" 武器碰撞体 " + weaponCollider.name);
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
    }
    // 实现基类的抽象方法
    protected override void UpdateBehavior()
    {
        // EnemyController使用状态机，这里不需要额外的行为逻辑
        // 状态机的更新在FSM类中处理
    }

    // 判断敌人类型并返回对应的状态工厂
    private IStateFactory GetStateFactory()
    {
        if (data is MeleeEnemyData) return new MeleeStateFactory();
        if (data is RangedEnemyData) return new RangedStateFactory();
        if (data is RunToneFlyingInsectData) return new RunToneFlyingStateFactory();

        //if (data is ExplosiveEnemyData) return new ExplosiveStateFactory();
        return null;
    }

    /// <summary>
    /// 敌人受伤 切换状态
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="rank"></param>
    public override void Wound(float damage, RhythmRank rank)
    {
        if (runtime.getHit) return;
        runtime.getHit = true;
        runtime.currentHealth -= damage;

        ShowDamageText(transform.position, damage, rank);
        fsm?.ChangeState(StateType.Wound);
    }

    // 死亡处理
    public void Dead()
    {
        StartCoroutine(DeadCoroutine());
    }

    // 死亡协程，延迟销毁以确保受伤状态有足够时间处理
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

        //// 播放死亡特效（使用安全的属性访问器）
        //if (runtime?.DeadEff != null)
        //    Instantiate(runtime.DeadEff, transform.position, transform.rotation);
    }


    public void OnPlayerEnter(Transform player)
    {
        if (runtime != null)
        {
            runtime.target = player;

        }
    }

    // 玩家离开时调用
    public void OnPlayerExit(Transform player)
    {
        if (runtime != null && runtime.target == player)
        {
            runtime.target = null;
        }
    }

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
    }
    public void FaceTarget(Vector2 targetPosition)
    {
        isFacingRight = targetPosition.x > transform.position.x;
        Vector3 rotation = transform.eulerAngles;
        rotation.y = isFacingRight ? 180 : 0;
        transform.eulerAngles = rotation;
    }

    #region  动画回调函数
    public void OnAttackAnimationEvent()
    {

        weaponCollider.enabled = true;

        var hitScript = weaponCollider.GetComponent<EnemyWeaponHit>();
        if (hitScript != null)
            hitScript.ResetHitFlag();
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


    public override void SetTarget(Transform target)
    {
        if (runtime != null)
            runtime.target = target;
    }
    public void ShowAttackWarning()
    {
        if (warningUI != null)
            warningUI.PlayWarning();
    }
}