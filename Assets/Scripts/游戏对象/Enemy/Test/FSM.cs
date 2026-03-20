using System;
using System.Collections.Generic;
using System.Data.Common;
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

public struct EnemyDiedStruct
{

}


#region 敌人的参数
[Serializable]
public class Parameter
{
    [Header("★ 基础信息")]
    public EnemyType enemyType;          // 用于在 FSM 中做类型判断

    [Header("🎮 数据配置")]
    public EnemyData data;               // 运行时引用对应的 ScriptableObject

    // 运行时状态（由代码自动管理）
    public Transform target;
    public bool getHit;
}
#endregion

public class FSM : MonoBehaviour
{
    #region 数据缓存
    // 公共数据（所有敌人共用）
    public EnemyData CommonData => parameter.data;

    // 具体类型数据（可能为 null，使用时需判断类型）
    public MeleeEnemyData MeleeData => CommonData as MeleeEnemyData;
    public RangedEnemyData RangedData => CommonData as RangedEnemyData;
    public ExplosiveEnemyData ExplosiveData => CommonData as ExplosiveEnemyData;

    // 常用的公共属性（直接从 CommonData 取，避免重复转换）
    public float MoveSpeed => CommonData != null ? CommonData.moveSpeed : 5f;
    public float ChaseSpeed => CommonData != null ? CommonData.chaseSpeed : 7f;
    public float IdleTime => CommonData != null ? CommonData.idleTime : 2f;
    public SpriteRenderer SpriteRenderer => CommonData?.spriteRenderer;
    public Animator Animator => CommonData?.animator;
    public GameObject DamageTextPrefab => CommonData?.damageTextPrefab;
    public GameObject DeadEff => CommonData?.deadEff;
    public Collider2D ChaseArea => CommonData?.chaseArea;
    public LayerMask TargetLayer => CommonData?.targetLayer ?? 0;

    // 近战专用
    public float MeleeAttackRange => MeleeData != null ? MeleeData.attackRange : 0f;
    public int MeleeAttackDamage => MeleeData != null ? MeleeData.attackDamage : 0;
    public Vector2 MeleeAttackOffset => MeleeData != null ? MeleeData.attackOffset : Vector2.zero;
    public Transform MeleeAttackPoint => MeleeData != null ? MeleeData.attackPoint : null;

    // 远程专用
    public float RangedAttackRange => RangedData != null ? RangedData.attackRange : 0f;
    public WeaponInfo RangedWeapon => RangedData != null ? RangedData.rangedWeapon : null;

    // 自爆专用
    public float ExplosionRadius => ExplosiveData != null ? ExplosiveData.explosionRadius : 0f;
    public int ExplosionDamage => ExplosiveData != null ? ExplosiveData.explosionDamage : 0;
    public GameObject ExplosionEffect => ExplosiveData != null ? ExplosiveData.explosionEffectPrefab : null;

    #endregion



    public IState currentState;
    public Parameter parameter;
    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    void Start()
    {
  
            // 确保数据已配置
            if (parameter.data == null)
            {
                Debug.LogError($"{gameObject.name} 的 Parameter.data 未配置！");
                return;
            }

            // 获取 SpriteRenderer
            if (parameter.data.spriteRenderer == null)
                parameter.data.spriteRenderer = GetComponent<SpriteRenderer>();

        // 获取 Animator
        if (parameter.data.animator == null)
            parameter.data.animator = GetComponent<Animator>();

        
        
       
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
               bullet.DestroyMyself();
               Wound(bullet.damage);
            }
        }
    }

    public void Dead()
    {
        // 通知所属房间（如果敌人有 ownerRoom 属性）
        parameter.data?.ownerRoom?.UnregisterEnemy(this);

        EventBus.Instance.Trigger(new EnemyDiedStruct());
        if (DeadEff != null)
            Instantiate(DeadEff, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    #region 绘制近战攻击范围
    private void OnDrawGizmos()
    {
        // 只对近战敌人绘制攻击范围
        if (parameter.enemyType == EnemyType.Melee && MeleeAttackPoint != null)
        {
            Vector2 pos = Application.isPlaying ? GetAttackWorldPos() : (Vector2)MeleeAttackPoint.position;
            Gizmos.DrawWireSphere(pos, MeleeAttackRange);
        }
    }

#endregion

#region 动态获得攻击点位置的方法（考虑朝向）
public Vector2 GetAttackWorldPos()
{
    if (parameter.enemyType == EnemyType.Melee && MeleeData != null)
    {
        float dir = SpriteRenderer.flipX ? -1f : 1f;
        return (Vector2)transform.position + new Vector2(dir * MeleeAttackOffset.x, MeleeAttackOffset.y);
    }
    return transform.position; // 保底
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
        if (DamageTextPrefab == null) return;
        GameObject dmgObj = Instantiate(DamageTextPrefab, position, Quaternion.identity);
        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
        if (dmgNumber != null) dmgNumber.SetDamage(damageValue);
    }
    #endregion

    #region 让敌人武器转向
    private void UpdateWeaponAim()
    {
        if (parameter.enemyType != EnemyType.Ranged) return;
        if (RangedWeapon == null || parameter.target == null) return;

        Transform weaponTransform = RangedWeapon.transform;
        Vector2 direction = parameter.target.position - weaponTransform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weaponTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    #endregion

}
