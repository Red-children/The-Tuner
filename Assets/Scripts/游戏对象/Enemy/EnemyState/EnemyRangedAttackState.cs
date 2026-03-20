using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangedAttackState : EnemyStateBase
{
    public EnemyRangedAttackState(FSM manager):base(manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public override void OnStart()
    {
        Debug.Log("进入远程攻击状态");
        parameter.animator.SetTrigger("Attack");
    }

    public override void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null) { manager.ChangeState(StateType.Patrol); return; }

        float distance = Vector2.Distance(manager.transform.position, parameter.target.position);
        float attackRange = parameter.attackRange;

        // 如果太远或太近，回到接近状态
        if (distance > attackRange || distance < attackRange * 0.6f)
        {
            manager.ChangeState(StateType.Approach);
            return;
        }

        // 在有效范围内，使用武器射击
        if (parameter.rangedWeapon != null)
        {
            parameter.rangedWeapon.Shoot(); // 武器内部处理冷却
        }
    }

    public override void OnExit() { }

    // 动画事件调用（如果需要）
    public void OnAttackHit() { }
}