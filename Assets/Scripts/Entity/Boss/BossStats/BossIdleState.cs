using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossIdleState : IState
{
    
    private BossController controller; // 新增
    private BossFSM fsm;
    private BossRuntime runtime; // 新增


    public BossIdleState(BossController bossController)
    {
        this.controller = bossController;
        this.runtime = bossController.runtime; // 获取运行时数据引用
        this.fsm = bossController.manager; // 获取状态机引用
    }

    public void OnStart() => Debug.Log("Boss进入Idle状态");
    public void OnUpdate()
    {
        if (runtime.target != null)
        {
            fsm.ChangeState(StateType.Chase);
            Debug.Log("Boss从Idle切换到Chase状态");
        }
    }
    public void OnExit() => Debug.Log("Boss退出Idle状态");
}
