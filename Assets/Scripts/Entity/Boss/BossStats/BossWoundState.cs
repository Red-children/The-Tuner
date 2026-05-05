using UnityEngine;

public class BossWoundState : EnemyStateBase
{
    private BossData bossData;
    private float timer;
    private float stunDuration = 0.5f;

    public BossWoundState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        bossData = data as BossData;
        manager.animator.SetTrigger("Wound");
        runtime.getHit = false;
        timer = 0f;
    }

    public override void OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= stunDuration)
        {
            if (runtime.currentHealth <= 0)
            {
                manager.ChangeState(StateType.Dead);
                return;
            }

            if (bossData != null && ShouldChangePhase())
            {
                manager.ChangeState(StateType.PhaseChange);
                return;
            }

            if (runtime.target != null)
                manager.ChangeState(StateType.Chase);
            else
                manager.ChangeState(StateType.Idle);
        }
    }

    private bool ShouldChangePhase()
    {
        float healthPercent = runtime.currentHealth / bossData.health;
        return healthPercent <= bossData.phaseChangeHealthThreshold;
    }

    public override void OnExit() { }
}
