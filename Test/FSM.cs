using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 注册所有状态
/// </summary>
public enum StateType
{
    Idle,   //等待
    Patrol, //巡逻
    Chase,  //追逐
    React,  //反应
    Attack, //攻击
    Wound,    //受击
    Dead     //死亡
}

[Serializable]//让Unity序列化这个类
//参数类
public class Parameter
{
    public GameObject DeadEff;  //死亡特效如果有的话 


    public Collider2D chesalArea;//巡逻范围 这里用2D碰撞体表示


    public Transform patrolCenter;   // 巡逻中心点（比如敌人的出生点或一个空物体）
    public float patrolRadius = 5f;  // 巡逻半径，可在 Inspector 中调整

    public int health;              //生命值

    public float moveSpeed;         //移动速度

    public float chaseSpeed;        //追逐速度

    public float idleTime;          //等待时间

    public Transform[] patrolPoints;//巡逻点

    public Transform[] chasePoints;//追逐点

    public Animator animator;       //动画组件

    public Transform target;       //玩家位置

    public bool getHit;           //是否受击

    public LayerMask targatLayer;   //玩家所在的层级
    public Transform attackPoint;     //攻击点位置

    public float attatkRange;        //攻击范围

}




public class FSM : MonoBehaviour
{
    

    //当前的状态
    private IState currentState;

    public Parameter parameter;

    //注册所有状态
    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    void Start()
    {
        //初始化各个状态，并添加到字典中
        states.Add(StateType.Idle, new IdleState(this));
        states.Add(StateType.Patrol, new PatrolState(this));
        states.Add(StateType.Chase, new ChaseState(this));
        states.Add(StateType.React, new ReactState(this));
        states.Add(StateType.Attack, new AttackState(this));
        states.Add(StateType.Wound, new WoundState(this));
        states.Add(StateType.Dead, new DeadState(this) );
        
        ChangeState(StateType.Idle);

        parameter.animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        currentState.OnUpdate();
    }

    //切换状态
    public void ChangeState(StateType newState)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }
        currentState = states[newState];
        currentState.OnStart();
    }

    public virtual void LookAtTarget(Transform target)
    {
        if (target == null) return;
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    //接受 Vector2（重载）
    public void LookAtTarget(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void OnPlayerEnter(Transform player)
    {
        parameter.target = player;
        // 可能还需要触发状态切换（比如直接进入 Chase）
        // 但你的状态机每帧检测 parameter.target，所以只需要设置即可
    }

    public void OnPlayerExit(Transform player)
    {
        if (parameter.target == player)
            parameter.target = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(parameter.attackPoint.position,parameter.attatkRange);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            parameter.getHit = true;
        }
    }

    public void Dead() 
    {
        Destroy(this.gameObject);
        //这里可以添加死亡特效，例如实例化死亡特效预制体
    }

}
