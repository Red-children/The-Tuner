using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

public class IdleState : IState
{
    private FSM manager;
    private Parameter parameter;

    public IdleState(FSM manager) 
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    private float timer;

    public void OnExit()
    {
        timer = 0;
    }

    public void OnStart()
    {
        //parameter.animator.Play("Idle");
        Debug.Log("进入Idle状态");
    }

    public void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Hit);
        }
        timer += Time.deltaTime;
        if(timer >= parameter.idleTime)
        {
            timer = 0;
            manager.ChangeState(StateType.Patrol);
        }
        if(parameter.target!= null)
        {
            manager.ChangeState(StateType.Chase);
        }
        

    }

}

public class PatrolState : IState
{
    private FSM manager;
    private Parameter parameter;
    private Vector2 targetPosition;
    private float minDistance = 0.1f;

    public PatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        //parameter.animator.Play("Walk");
        Debug.Log("进入Patrol状态");
        GetNewRandomTarget(); // 进入状态时获取第一个随机点
    }

    public void OnUpdate()
    {
        // 面向目标点（可选）
        manager.LookAtTarget(targetPosition); // 需要你自己实现面向向量的方法

        if (targetPosition != Vector2.zero)
        {
            // 向目标点移动
            manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            targetPosition,
            parameter.moveSpeed * Time.deltaTime);
        }
        // 到达目标点后，获取下一个随机点
        if (Vector2.Distance(manager.transform.position, targetPosition) < minDistance)
        {
            GetNewRandomTarget();
        }

        // 原有的受击和发现玩家逻辑
        if (parameter.getHit)
            manager.ChangeState(StateType.Hit);
        if(parameter.target!=null)
            manager.ChangeState(StateType.Chase);
    }

    public void OnExit()
    {
        targetPosition = Vector2.zero;
    }

    private void GetNewRandomTarget()
    {
        if (parameter.patrolCenter == null)
        {
            Debug.LogWarning("巡逻中心点未设置！");
            return;
        }
        Vector2 center = parameter.patrolCenter.position;
        // 在圆内随机取点：随机角度和半径
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float radius = Random.Range(0f, parameter.patrolRadius);
        // 为了使随机点分布更均匀，可以用 sqrt 处理半径（可选）
        // float radius = Mathf.Sqrt(Random.value) * parameter.patrolRadius;
        Vector2 offset = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        targetPosition = center + offset;
    }
}

public class ChaseState : IState
{
    private FSM manager;
    private Parameter parameter;
    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnExit()
    {
        
    }
    public void OnStart()
    {
        //parameter.animator.Play("Walk");
        Debug.Log("进入Chase状态");
    }
    public void OnUpdate()
    {
        if (parameter.target == null)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }
        manager.LookAtTarget(parameter.target.position); // 面向玩家
        if(parameter.target != null)
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                parameter.target.position,
                parameter.moveSpeed * Time.deltaTime);
        }
        
        if (Physics2D.OverlapCircle(parameter.attackPoint.position,parameter.attatkRange,parameter.targatLayer)) 
        {
            manager.ChangeState(StateType.Attack);
        }

    }
}

public class ReactState : IState
{
    private FSM manager;
    private Parameter parameter;
    public ReactState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnExit()
    {
        throw new System.NotImplementedException();
    }
    public void OnStart()
    {
        throw new System.NotImplementedException();
    }
    public void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}

public class AttackState : IState
{
    private FSM manager;
    private Parameter parameter;
    //用来获取动画状态信息，判断动画是否播放完毕
    private AnimatorStateInfo info;
    public AttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnExit()
    {
        
    }
    public void OnStart()
    {
        //parameter.animator.Play("Attack");
        Debug.Log("进入Attack状态");
    }
    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Hit);
        }
        if (info.normalizedTime >= .95f)
        {
            manager.ChangeState(StateType.Chase);
        }
    }
}


