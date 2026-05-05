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

        // 纯粹远程：进入攻击范围就切换到远程攻击
        if (distance <= bossData.attackRange)
        {
            manager.ChangeState(StateType.BossRangedAttack);
            return;
        }
    }

    public override void OnExit() { }
}
