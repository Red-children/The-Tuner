using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackState : IState
{
    private BossController controller; // 新增
    private BossFSM fsm;
    private BossRuntime runtime; // 新增
    public BossAttackState(BossController bossController)
    {
        this.controller = bossController;
        this.runtime = bossController.runtime; // 获取运行时数据引用
        this.fsm = bossController.manager; // 获取状态机引用
    }

    public void OnStart()
    {
        // 进入Attack状态时的逻辑，例如播放攻击动画
        Debug.Log("Boss进入Attack状态");
    }

    public void OnExit()
    {
        // 退出Attack状态时的逻辑，例如停止攻击动画
        Debug.Log("Boss退出Attack状态");
    }

    public void OnUpdate()
    {
    }
}
