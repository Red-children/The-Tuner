using UnityEngine;

public class BossChaseState : EnemyStateBase
{
    private BossData bossData;
    private float skillCheckTimer;

    public BossChaseState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        bossData = data as BossData;
        manager.animator.SetTrigger("Move");
        controller.agent.speed = data.chaseSpeed;
        skillCheckTimer = 0f;
    }

    public override void OnUpdate()
    {
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (runtime.target == null) { manager.ChangeState(StateType.Idle); return; }

        controller.FaceTarget(runtime.target.position);
        controller.agent.SetDestination(runtime.target.position);

        float distance = Vector2.Distance(controller.transform.position, runtime.target.position);

        if (bossData != null)
        {
            skillCheckTimer += Time.deltaTime;
            if (skillCheckTimer >= 1f)
            {
                skillCheckTimer = 0f;
                if (distance >= bossData.skillMinRange && distance <= bossData.skillMaxRange)
                {
                    if (Time.time >= bossData.skillCooldown)
                    {
                        manager.ChangeState(StateType.Skill);
                        return;
                    }
                }
            }

            if (distance <= bossData.normalAttackRange)
            {
                manager.ChangeState(StateType.Attack);
            }
        }
    }

    public override void OnExit() { }
}
