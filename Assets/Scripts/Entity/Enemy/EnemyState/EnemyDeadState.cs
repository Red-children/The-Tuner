using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
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

    }

    /// <summary>
    /// 由动画事件调用，死亡动画播放完毕后销毁敌人
    /// </summary>
    public void OnDeathAnimationFinished()
    {
        if (deathTriggered) return;
        deathTriggered = true;
        Debug.Log($"[{controller.name}] 死亡动画结束，销毁敌人");

        if (controller.runtime?.DeadEff != null)
           UnityEngine.Object.Instantiate(controller.runtime.DeadEff, controller.transform.position, Quaternion.identity);

        UnityEngine.Object.Destroy(controller.gameObject);
    }
}


