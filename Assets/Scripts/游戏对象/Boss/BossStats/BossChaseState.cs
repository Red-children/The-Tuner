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
    }

    public void OnUpdate()
    {
        // 没有目标就回 Idle
        if (runtime.target == null)
        {
            fsm.ChangeState(StateType.Idle);
            return;
        }

        // 如果受到伤害，切到 Wound（后面实现）
        if (runtime.getHit)
        {
            fsm.ChangeState(StateType.Wound);
            return;
        }

        // 计算方向并移动
        Vector2 direction = (runtime.target.position - controller.transform.position).normalized;
        controller.transform.Translate(direction * runtime.currentChaseSpeed * Time.deltaTime);

        // 翻转精灵
        if (direction.x != 0)
            controller.spriteRenderer.flipX = direction.x < 0;

        // 如果距离小于攻击范围，切到 Attack（后面实现）
        float distance = Vector2.Distance(controller.transform.position, runtime.target.position);
        if (distance <= runtime.Data.normalAttackRange) // 注意：BossData 里要有 normalAttackRange
        {
            fsm.ChangeState(StateType.Attack);
        }
    }

    public void OnExit()
    {
        Debug.Log("Boss 退出 Chase 状态");
    }
}