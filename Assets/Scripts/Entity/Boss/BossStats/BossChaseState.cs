using UnityEngine;

public class BossChaseState : EnemyStateBase
{
    private BossData bossData;

    public BossChaseState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        bossData = data as BossData;
        manager.animator.SetTrigger("Move");
        controller.agent.speed = data.chaseSpeed;
    }

    public override void OnUpdate()
    {
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

        controller.FaceTarget(runtime.target.position);
        controller.agent.SetDestination(runtime.target.position);

        float distance = Vector2.Distance(controller.transform.position, runtime.target.position);

        if (bossData == null) return;

        if (distance <= bossData.meleeAttackRange)
        {
            manager.ChangeState(StateType.Approach);
            return;
        }

        if (distance <= bossData.rangedAttackRange)
        {
            if (Random.value > 0.5f)
            {
                manager.ChangeState(StateType.Approach);
            }
            else
            {
                manager.ChangeState(StateType.BossRangedAttack);
            }
        }
    }

    public override void OnExit() { }
}
