using UnityEngine;

public class BossPhaseChangeState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;
    private float phaseTimer;
    private bool phaseCompleted;

    public BossPhaseChangeState(BossController bossController)
    {
        controller = bossController;
        fsm = bossController.manager;
        runtime = bossController.runtime;
    }

    public void OnStart()
    {
        phaseTimer = 0f;
        phaseCompleted = false;

        runtime.phaseInvincibleTimer = 5f;

        controller.animator?.SetTrigger("PhaseChange");
        ApplyPhaseUpgrade();
    }

    public void OnUpdate()
    {
        phaseTimer += Time.deltaTime;

        if (!phaseCompleted && phaseTimer >= 5f)
        {
            phaseCompleted = true;
            OnPhaseChangeFinished();
        }

        if (runtime.getHit)
            runtime.getHit = false;
    }

    private void ApplyPhaseUpgrade()
    {
        runtime.currentPhase++;
        runtime.hasPhaseChanged = true;
        runtime.currentMoveSpeed *= 1.2f;
        runtime.currentChaseSpeed *= 1.2f;
    }

    private void OnPhaseChangeFinished()
    {
        if (runtime.currentHealth <= 0)
        {
            fsm.ChangeState(BossStateType.Dead);
            return;
        }

        if (runtime.target != null)
            fsm.ChangeState(BossStateType.Chase);
        else
            fsm.ChangeState(BossStateType.Idle);
    }

    public void OnExit()
    {
        controller.animator?.ResetTrigger("PhaseChange");
    }
}