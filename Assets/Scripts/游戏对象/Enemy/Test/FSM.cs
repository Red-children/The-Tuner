using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject damageTextPrefab; // 伤害飘字预制体
    public SpriteRenderer spriteRenderer; // 拖入敌人的 SpriteRenderer
    public GameObject DeadEff;           // 死亡特效
    public Collider2D chaseArea;          // 追逐范围触发器
    public Transform patrolCenter;        // 巡逻中心点
    public Transform[] patrolPoints;        // 巡逻点数组
    public float patrolRadius = 5f;       // 巡逻半径
    public float health;                    // 当前生命值
    public float moveSpeed;               // 移动速度
    public float chaseSpeed;               // 追逐速度
    public float idleTime;                 // 空闲等待时间
    public Animator animator;              // 动画组件
    public Transform target;               // 当前目标（玩家）
    public bool getHit;                     // 是否受击
    public LayerMask targetLayer;           // 目标层级
    public Transform attackPoint;           // 攻击点
    public float attackRange;               // 攻击范围
    public int attackDamage = 10;           // 攻击力
    public Vector2 attackOffset = new Vector2(1f, 0f); // 攻击点相对于敌人中心的偏移（敌人默认朝右）
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
        states.Add(StateType.Idle, new IdleState(this));
        states.Add(StateType.Patrol, new PatrolState(this));
        states.Add(StateType.Chase, new ChaseState(this));
        states.Add(StateType.Attack, new AttackState(this));
        states.Add(StateType.Wound, new WoundState(this));
        states.Add(StateType.Dead, new DeadState(this));
        states.Add(StateType.Approach, new ApproachState (this));
        #endregion

        //开始先进入空闲状态
        ChangeState(StateType.Idle);
    }

    void Update()
    {
        //每帧调用当前状态的更新方法
        //相当于把Update的逻辑分散到各个状态中，保持 FSM 类的简洁
        currentState?.OnUpdate();
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
        (states[StateType.Wound] as WoundState).finallyDamage = damage;
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


}
