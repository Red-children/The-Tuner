using UnityEngine;

public class BossChaseState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;

    public BossChaseState(BossController bossController)
    {
        controller = bossController;
        fsm = bossController.manager;
        runtime = bossController.runtime;
    }

    public void OnStart()
    {
        Debug.Log("Boss 进入 Chase 状态");
        controller.PlayIdle(); // 可替换为移动动画
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

        Vector2 direction = (runtime.target.position - controller.transform.position).normalized;
        controller.transform.Translate(direction * runtime.currentChaseSpeed * Time.deltaTime);

        if (direction.x != 0)
            controller.spriteRenderer.flipX = direction.x < 0;

        float distance = Vector2.Distance(controller.transform.position, runtime.target.position);
        if (distance <= runtime.Data.normalAttackRange)
        {
            fsm.ChangeState(BossStateType.Attack);
        }
    }

    public void OnExit()
    {
        Debug.Log("Boss 退出 Chase 状态");
    }
}