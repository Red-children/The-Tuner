using UnityEngine;

public class BossWoundState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;
    private float timer;

    public BossWoundState(BossController bossController)
    {
        controller = bossController;
        fsm = bossController.manager;
        runtime = bossController.runtime;
    }

    public void OnStart()
    {
        runtime.getHit = false;
        timer = 0f;

        controller.animator?.SetTrigger("Hurt");
    }

    public void OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= runtime.Data.staggerDuration)
        {
            if (runtime.currentHealth <= 0)
            {
                fsm.ChangeState(BossStateType.Dead);
                return;
            }

            if (ShouldChangePhase() && !runtime.hasPhaseChanged)
            {
                fsm.ChangeState(BossStateType.PhaseChange);
                return;
            }

            runtime.superArmorTimer = 5f;

            if (runtime.target != null)
                fsm.ChangeState(BossStateType.Chase);
            else
                fsm.ChangeState(BossStateType.Idle);
        }
    }

    public void OnExit()
    {
    }

    private bool ShouldChangePhase()
    {
        if (runtime.Data == null) return false;
        float healthPercent = runtime.currentHealth / runtime.Data.health;
        return healthPercent <= runtime.Data.phaseChangeHealthThreshold;
    }
}