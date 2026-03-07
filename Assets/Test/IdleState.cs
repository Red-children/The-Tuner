using UnityEngine;

public class IdleState : IState
{
    private FSM manager;
    private Parameter parameter;
    private float timer;

    public IdleState(FSM manager)
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
        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= parameter.idleTime)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }

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

public class PatrolState : IState
{
    private FSM manager;
    private Parameter parameter;
    private Vector2 targetPos;
    private float minDistance = 0.1f;

    public PatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入Patrol状态");
        GetNewRandomTarget();
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

        manager.LookAtTarget(targetPos);
        manager.transform.position = Vector2.MoveTowards(manager.transform.position, targetPos, parameter.moveSpeed * Time.deltaTime);

        if (Vector2.Distance(manager.transform.position, targetPos) < minDistance)
        {
            GetNewRandomTarget();
        }
    }

    public void OnExit()
    {
        targetPos = Vector2.zero;
    }

    private void GetNewRandomTarget()
    {
        if (parameter.patrolCenter == null)
        {
            Debug.LogWarning("巡逻中心点未设置！");
            return;
        }
        Vector2 center = parameter.patrolCenter.position;
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float radius = Random.Range(0f, parameter.patrolRadius);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        targetPos = center + offset;
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

    public void OnStart()
    {
        Debug.Log("进入Chase状态");
    }

    public void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        if (parameter.target == null)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }

        manager.LookAtTarget(parameter.target);
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            parameter.target.position,
            parameter.chaseSpeed * Time.deltaTime);

        if (Physics2D.OverlapCircle(parameter.attackPoint.position, parameter.attackRange, parameter.targetLayer))
        {
            manager.ChangeState(StateType.Attack);
        }
    }

    public void OnExit() { }
}

public class AttackState : IState
{
    private FSM manager;
    private Parameter parameter;
    private float attackTimer;

    public AttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入Attack状态");
        attackTimer = 0f;
    }

    public void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        if (parameter.target == null || !IsTargetInRange())
        {
            manager.ChangeState(StateType.Chase);
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f) // 攻击间隔，可调整
        {
            attackTimer = 0f;
            PlayerIObject player = parameter.target.GetComponent<PlayerIObject>();
            //if (player != null)
            //{
            //    player.TakeDamage(parameter.attackDamage);
            //}
        }
    }

    private bool IsTargetInRange()
    {
        return Physics2D.OverlapCircle(parameter.attackPoint.position, parameter.attackRange, parameter.targetLayer) != null;
    }

    public void OnExit() { }
}

public class WoundState : IState
{
    private FSM manager;
    private Parameter parameter;
    private float timer;

    public float finallyDamage;

    public WoundState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入Wound状态");
        parameter.getHit = false;
        parameter.health -= finallyDamage; // 扣血
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

public class DeadState : IState
{
    private FSM manager;
    private Parameter parameter;

    public DeadState(FSM manager)
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