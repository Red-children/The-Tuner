using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[Serializable]

public class EnemyDeadState : EnemyStateBase
{
    private bool deathTriggered = false;

    public EnemyDeadState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        Debug.Log($"[{controller.name}] 进入死亡状态");
        
        // 停止 NavMeshAgent，避免死亡后还在移动
        NavMeshAgent agent = controller.agent;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // 触发死亡动画
        manager.animator.SetTrigger("Dead");
        
    }

    public override void OnUpdate()
    {
        // 死亡状态中无需任何逻辑，完全依赖动画事件驱动
    }

    public override void OnExit()
    {
        // 一般不需要退出，但保留清理
    }
}


