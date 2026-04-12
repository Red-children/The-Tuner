using UnityEngine;

public class BossIdleState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;

    public BossIdleState(BossController bossController)
    {
        controller = bossController;
        runtime = bossController.runtime;
        fsm = bossController.manager;
    }

    public void OnStart()
    {
        Debug.Log("Boss进入Idle状态");
        controller.PlayIdle();
    }

    public void OnUpdate()
    {
        if (runtime.target != null)
        {
            fsm.ChangeState(BossStateType.Chase);
        }
    }

    public void OnExit() => Debug.Log("Boss退出Idle状态");
}