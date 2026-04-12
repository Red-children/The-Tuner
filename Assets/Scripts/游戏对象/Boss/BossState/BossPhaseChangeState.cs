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
        Debug.Log("Boss 进入阶段切换状态");
        phaseTimer = 0f;
        phaseCompleted = false;

        controller.animator?.SetTrigger("PhaseChange");
        ApplyPhaseUpgrade();
    }

    public void OnUpdate()
    {
        phaseTimer += Time.deltaTime;

        float phaseChangeDuration = 2f;
        if (!phaseCompleted && phaseTimer >= phaseChangeDuration)
        {
            phaseCompleted = true;
            OnPhaseChangeFinished();
        }

        if (runtime.getHit)
        {
            runtime.getHit = false;
        }
    }

    private void ApplyPhaseUpgrade()
    {
        runtime.currentPhase++;
        runtime.hasPhaseChanged = true;
        runtime.currentMoveSpeed *= 1.2f;
        runtime.currentChaseSpeed *= 1.2f;
        Debug.Log("Boss 进入新阶段，属性增强！");
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
        Debug.Log("Boss 退出阶段切换状态");
        controller.animator?.ResetTrigger("PhaseChange");
    }
}