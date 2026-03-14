using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

#region 敌人枚举类型
public enum EnemyType
{
    Melee,   // 近战
    Ranged   // 远程
}
#endregion

#region 定义状态类型枚举
public enum StateType
{
    Idle,       // 空闲状态
    Patrol,     // 巡逻状态
    Chase,      // 追逐状态
    Attack,     // 攻击状态
    Wound,      // 受击状态
    Dead,       // 死亡状态
    Approach    // 接近状态（新增）用于优化追逐行为，在玩家进入追逐范围但未进入攻击范围时切换到该状态，调整朝向并准备攻击

}
#endregion

#region 敌人的参数
[Serializable]
public class Parameter
{
    [Header("★ 基础信息")]
    public EnemyType enemyType;          // 敌人类型（近战/远程）
    public float health;                 // 当前生命值（可在 Inspector 中设置初始值）
    public LayerMask targetLayer;        // 目标层级（通常是玩家）

    [Header("🏃 移动参数")]
    public float moveSpeed;              // 巡逻/常规移动速度
    public float chaseSpeed;              // 追逐玩家时的速度
    public float idleTime;                // 空闲状态的等待时间

    [Header("📍 巡逻设置")]
    public Transform patrolCenter;        // 随机巡逻的中心点（当 patrolPoints 为空时使用）
    public Transform[] patrolPoints;      // 固定巡逻点数组（优先级高于随机巡逻）
    public float patrolRadius = 5f;       // 随机巡逻半径

    [Header("⚔️ 近战攻击设置")]
    public Transform attackPoint;          // 近战攻击点（用于检测范围）
    public float attackRange;              // 近战攻击范围
    public int attackDamage = 10;          // 近战攻击力
    public Vector2 attackOffset = new Vector2(1f, 0f); // 攻击点相对于敌人中心的偏移（朝向影响）

    [Header("🎯 远程攻击设置（仅对 Ranged 类型生效）")]
    public WeaponInfo rangedWeapon;        // 远程敌人挂载的武器组件

    [Header("✨ 视觉与特效")]
    public GameObject damageTextPrefab;    // 伤害飘字预制体
    public GameObject DeadEff;              // 死亡特效预制体
    public SpriteRenderer spriteRenderer;   // 用于翻转角色朝向
    public Animator animator;               // 动画控制器

    [Header("🔍 触发器引用")]
    public Collider2D chaseArea;            // 追逐范围触发器（用于检测玩家进入）

    [Header("⏱️ 运行时状态（由代码自动管理）")]
    [HideInInspector] public Transform target;   // 当前目标（玩家）
    [HideInInspector] public bool getHit;         // 是否受击（用于切换到受击状态）
}
#endregion

public class FSM : MonoBehaviour
{
    public IState currentState;
    public Parameter parameter;
    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    void Start()
    {
        #region 动态获取组件的代码 允许在 Inspector 中拖入组件，如果未拖入则自动获取
        if (parameter.spriteRenderer == null)
        parameter.spriteRenderer = GetComponent<SpriteRenderer>();

        if (parameter.animator == null)
            parameter.animator = GetComponent<Animator>();
        #endregion

        #region 注册所有的状态实例 这里可以根据需要添加更多状态
            // ... 通用状态注册 ...

            // 根据类型注册攻击状态
            if (parameter.enemyType == EnemyType.Melee)
                states.Add(StateType.Attack, new EnemyMeleeAttackState(this));
            else
                states.Add(StateType.Attack, new EnemyRangedAttackState(this));

            // 根据类型注册接近状态（如果需要）
            if (parameter.enemyType == EnemyType.Melee)
                states.Add(StateType.Approach, new EnemyMeleeApproachState(this));
            else
                states.Add(StateType.Approach, new EnemyRangedApproachState(this));

            states.Add(StateType.Idle, new EnemyIdleState(this));
        states.Add(StateType.Patrol, new EnemyPatrolState(this));
        states.Add(StateType.Chase, new EnemyChaseState(this));
        states.Add(StateType.Wound, new EnemyWoundState(this));
        states.Add(StateType.Dead, new EnemyDeadState(this));
        
        #endregion

        //开始先进入空闲状态
        ChangeState(StateType.Idle);
    }

    void Update()
    {
        //每帧调用当前状态的更新方法
        //相当于把Update的逻辑分散到各个状态中，保持 FSM 类的简洁
        currentState?.OnUpdate();

        // 武器朝向逻辑（放在这里，独立于状态）
        UpdateWeaponAim();
    }

    public void ChangeState(StateType newState)
    {
        currentState?.OnExit();
        currentState = states[newState];
        currentState.OnStart();
    }


    #region 由其他碰撞器检测玩家进入的方法，供 TriggerForward 调用
    public void OnPlayerEnter(Transform player)
    {
        parameter.target = player;
    }

    public void OnPlayerExit(Transform player)
    {
        if (parameter.target == player)
            parameter.target = null;
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            parameter.getHit = true;
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
               Wound(bullet.damage);
            }
        }
    }

    public void Dead()
    {
        if (parameter.DeadEff != null)
            Instantiate(parameter.DeadEff, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (parameter.attackPoint != null)
        {
            Vector2 pos = Application.isPlaying ? GetAttackWorldPos() : (Vector2)parameter.attackPoint.position;
            Gizmos.DrawWireSphere(pos, parameter.attackRange);
        }
    }

    #region 动态获得攻击点位置的方法（考虑朝向）
    public Vector2 GetAttackWorldPos()
    {
        float dir = parameter.spriteRenderer.flipX ? -1f : 1f;
        return (Vector2)transform.position + new Vector2(dir * parameter.attackOffset.x, parameter.attackOffset.y);
    }
    #endregion

    #region 切换到受击状态的方法 这里可以进行伤害的计算 考虑敌人的减伤相关
    public void Wound(float damage) 
    {
        (states[StateType.Wound] as EnemyWoundState).finallyDamage = damage;
        ChangeState(StateType.Wound);
        
    }
    #endregion

    
    
    
    #region 显示伤害飘字的方法（需要在 Parameter 中添加 damageTextPrefab）
    public void ShowDamageText(Vector3 position, float damageValue)
    {
        if (parameter.damageTextPrefab == null) return; // 需要在 Parameter 中添加 damageTextPrefab
        GameObject dmgObj = Instantiate(parameter.damageTextPrefab, position, Quaternion.identity) ;
        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
        if (dmgNumber != null)
            dmgNumber.SetDamage(damageValue);
    }
    #endregion

    private void UpdateWeaponAim()
    {
        if (parameter.rangedWeapon == null || parameter.target == null)
            return;

        // 获取武器物体的 Transform（通常 WeaponInfo 挂在武器物体上）
        Transform weaponTransform = parameter.rangedWeapon.transform;

        // 计算从武器位置指向目标的方向
        Vector2 direction = parameter.target.position - weaponTransform.position;

        // 计算角度（2D 游戏，绕 Z 轴旋转）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 应用旋转
        weaponTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
