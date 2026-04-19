using UnityEngine;
using System;

[Serializable]
public class EnemyMeleeAttackState : EnemyStateBase
{
    public EnemyMeleeAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        controller.ShowAttackWarning();
        manager.animator.SetTrigger("Attack");
        if (runtime.target == null)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }
        // 不再启动任何协程
    }

    public override void OnUpdate()
    {
        // 受击打断攻击
        if (runtime.getHit)
        {
            manager.ChangeState(StateType.Wound);
        }
    }

    public override void OnExit()
    {
        // 确保武器碰撞体关闭
        if (controller.weaponCollider != null)
            controller.weaponCollider.enabled = false;
    }
}