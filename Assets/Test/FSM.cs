using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public enum StateType
{
    Idle, Patrol, Chase, Attack, Wound, Dead
}

[Serializable]
public class Parameter
{
    public GameObject DeadEff;           // 死亡特效
    public Collider2D chaseArea;          // 追逐范围触发器
    public Transform patrolCenter;        // 巡逻中心点
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
}

public class FSM : MonoBehaviour
{
    private IState currentState;
    public Parameter parameter;
    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    void Start()
    {
        states.Add(StateType.Idle, new IdleState(this));
        states.Add(StateType.Patrol, new PatrolState(this));
        states.Add(StateType.Chase, new ChaseState(this));
        states.Add(StateType.Attack, new AttackState(this));
        states.Add(StateType.Wound, new WoundState(this));
        states.Add(StateType.Dead, new DeadState(this));

        if (parameter.animator == null)
            parameter.animator = GetComponent<Animator>();

        ChangeState(StateType.Idle);
    }

    void Update()
    {
        currentState?.OnUpdate();
    }

    public void ChangeState(StateType newState)
    {
        currentState?.OnExit();
        currentState = states[newState];
        currentState.OnStart();
    }

    public void LookAtTarget(Transform target)
    {
        if (target == null) return;
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void LookAtTarget(Vector2 targetPos)
    {
        Vector2 dir = targetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void OnPlayerEnter(Transform player)
    {
        parameter.target = player;
    }

    public void OnPlayerExit(Transform player)
    {
        if (parameter.target == player)
            parameter.target = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            parameter.getHit = true;
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                // 先设置伤害值
                (states[StateType.Wound] as WoundState).finallyDamage = bullet.damage;
                // 再切换状态
                ChangeState(StateType.Wound);
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
            Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.attackRange);
    }
}
