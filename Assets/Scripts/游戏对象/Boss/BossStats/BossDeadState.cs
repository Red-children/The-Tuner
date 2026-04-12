using UnityEngine;

[System.Serializable]
public class BossDeadState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;

    public BossDeadState(BossController bossController)
    {
        controller = bossController;
        fsm = bossController.manager;
        runtime = bossController.runtime;
    }

    public void OnStart()
    {
        Debug.Log("Boss 进入 Dead 状态");
        // 死亡逻辑已由 controller.DeathCoroutine 处理，这里只需启动协程
        controller.StartCoroutine(controller.DeathCoroutine());
    }

    public void OnUpdate() { }
    public void OnExit() { }
}