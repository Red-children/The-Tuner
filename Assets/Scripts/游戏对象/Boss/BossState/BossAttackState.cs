using UnityEngine;

public class BossAttackState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;
    private float attackTimer;

    public BossAttackState(BossController bossController)
    {
        controller = bossController;
        runtime = bossController.runtime;
        fsm = bossController.manager;
    }

    public void OnStart()
    {
        Debug.Log("Boss进入Attack状态");
        attackTimer = 0f;
    }

    public void OnUpdate()
    {
        if (runtime.target == null)
        {
            fsm.ChangeState(BossStateType.Idle);
            return;
        }

        if (runtime.getHit)
        {
            fsm.ChangeState(BossStateType.Wound);
            return;
        }

        float distance = Vector3.Distance(controller.transform.position, runtime.target.position);

        if (controller.skill != null && controller.skill.CanUseSkill())
        {
            controller.skill.UseRandomSkill();
            fsm.ChangeState(BossStateType.Skill);
            return;
        }

        if (distance <= runtime.Data.normalAttackRange)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f)
            {
                PerformAttack();
                attackTimer = 0f;
            }
        }
        else
        {
            fsm.ChangeState(BossStateType.Chase);
        }
    }

    private void PerformAttack()
    {
        controller.PlayAttack();
        Debug.Log("Boss 执行普通攻击，造成伤害: " + runtime.Data.specialAttackDamage);
    }

    public void OnExit()
    {
        Debug.Log("Boss退出Attack状态");
    }
}