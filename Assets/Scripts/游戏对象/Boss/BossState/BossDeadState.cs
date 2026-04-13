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
        // 开启死亡协程，处理死亡动画、掉落物品等逻辑
        controller.StartCoroutine(controller.DeathCoroutine());
    }

    public void OnUpdate() { }
    public void OnExit() { }
}