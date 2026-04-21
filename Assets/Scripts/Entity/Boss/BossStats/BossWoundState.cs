using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

[System.Serializable]
public class BossWoundState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;

    private float timer;
    private float stunDuration = 0.5f; // 硬直时间，可配置

    public BossWoundState(BossController bossController)
    {
        controller = bossController;
        fsm = bossController.manager;
        runtime = bossController.runtime;
    }

    public void OnStart()
    {
        Debug.Log("Boss 进入 Wound 状态");
        runtime.getHit = false; // 重置受击标记，避免再次进入
        timer = 0f;

        // 可在此播放受击动画
        // controller.animator?.SetTrigger("Hurt");
    }

    public void OnUpdate()
    {
        timer += Time.deltaTime;

        // 硬直结束后判断下一步
        if (timer >= stunDuration)
        {
            if (runtime.currentHealth <= 0)
            {
                fsm.ChangeState(StateType.Dead);
                return;
            }

            // 检查是否需要切换阶段（血量低于阈值）
            if (ShouldChangePhase())
            {
                // 如果有阶段切换状态，就切过去；否则直接回 Chase
                // fsm.ChangeState(StateType.PhaseChange);
                // return;
            }

            // 有目标则追逐，无目标则 Idle
            if (runtime.target != null)
                fsm.ChangeState(StateType.Chase);
            else
                fsm.ChangeState(StateType.Idle);
        }
    }

    public void OnExit()
    {
        Debug.Log("Boss 退出 Wound 状态");
    }

    private bool ShouldChangePhase()
    {
        if (runtime.Data == null) return false;
        float healthPercent = runtime.currentHealth / runtime.Data.health;
        return healthPercent <= runtime.Data.phaseChangeHealthThreshold;
    }
}
