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
    private float stunDuration = 0.5f; // Ӳֱʱ�䣬������

    public BossWoundState(BossController bossController)
    {
        controller = bossController;
        fsm = bossController.manager;
        runtime = bossController.runtime;
    }

    public void OnStart()
    {
        Debug.Log("Boss ���� Wound ״̬");
        runtime.getHit = false; // �����ܻ���ǣ������ٴν���
        timer = 0f;

        // ���ڴ˲����ܻ�����
        // controller.animator?.SetTrigger("Hurt");
    }

    public void OnUpdate()
    {
        timer += Time.deltaTime;

        // Ӳֱ�������ж���һ��
        if (timer >= stunDuration)
        {
            if (runtime.currentHealth <= 0)
            {
                fsm.ChangeState(StateType.Dead);
                return;
            }

            // ����Ƿ���Ҫ�л��׶Σ�Ѫ��������ֵ��
            if (ShouldChangePhase())
            {
                // ����н׶��л�״̬�����й�ȥ������ֱ�ӻ� Chase
                // fsm.ChangeState(StateType.PhaseChange);
                // return;
            }

            // ��Ŀ����׷����Ŀ���� Idle
            if (runtime.target != null)
                fsm.ChangeState(StateType.Chase);
            else
                fsm.ChangeState(StateType.Idle);
        }
    }

    public void OnExit()
    {
        Debug.Log("Boss �˳� Wound ״̬");
    }

    private bool ShouldChangePhase()
    {
        if (runtime.Data == null) return false;
        float healthPercent = runtime.currentHealth / runtime.Data.health;
        return healthPercent <= runtime.Data.phaseChangeHealthThreshold;
    }
}
