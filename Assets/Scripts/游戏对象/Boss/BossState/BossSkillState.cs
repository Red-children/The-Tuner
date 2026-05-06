using UnityEngine;

public class BossSkillState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;
    private bool skillUsed;
    private float originMoveSpeed;
    private float originChaseSpeed;

    public BossSkillState(BossController bossController)
    {
        controller = bossController;
        runtime = bossController.runtime;
        fsm = bossController.manager;
    }

    public void OnStart()
    {
        Debug.Log("Boss进入Skill状态");

        skillUsed = false;
        runtime.superArmorTimer = 999f;

        // 记录原速度
        originMoveSpeed = runtime.currentMoveSpeed;
        originChaseSpeed = runtime.currentChaseSpeed;
    }

    public void OnUpdate()
    {
        if (runtime.target == null)
        {
            runtime.superArmorTimer = 0f;
            fsm.ChangeState(BossStateType.Idle);
            return;
        }

        if (!skillUsed)
        {
            controller.skill.UseRandomSkill();
            skillUsed = true;
        }

        if (skillUsed)
        {
            runtime.superArmorTimer = 0f;

            float distance = Vector3.Distance(controller.transform.position, runtime.target.position);

            if (distance <= runtime.Data.normalAttackRange)
                fsm.ChangeState(BossStateType.Attack);
            else
                fsm.ChangeState(BossStateType.Chase);
        }
    }

    public void OnExit()
    {
        runtime.superArmorTimer = 0f;

        //恢复移动
        runtime.currentMoveSpeed = runtime.Data.moveSpeed;
        runtime.currentChaseSpeed = runtime.Data.chaseSpeed;

        Debug.Log("技能状态退出，恢复移动");
    }
}