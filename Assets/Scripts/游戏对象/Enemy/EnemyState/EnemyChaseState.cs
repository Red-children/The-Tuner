using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyStateBase
{
    public EnemyChaseState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        Debug.Log("进入Chase状态");
    }

    public override void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null) { manager.ChangeState(StateType.Patrol); return; }

        // 面朝玩家
        bool flip = parameter.target.position.x < manager.transform.position.x;
        manager.SpriteRenderer.flipX = flip;

        // 移动
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            parameter.target.position,
            manager.ChaseSpeed * Time.deltaTime);

        // 根据敌人类型判断切换
        if (parameter.enemyType == EnemyType.Ranged)
        {
            float distance = Vector2.Distance(manager.transform.position, parameter.target.position);
            if (distance <= manager.RangedAttackRange)
            {
                manager.ChangeState(StateType.Approach);
            }
        }
        else // 近战
        {
            if (Physics2D.OverlapCircle(manager.GetAttackWorldPos(), manager.MeleeAttackRange, manager.TargetLayer))
            {
                manager.ChangeState(StateType.Approach);
            }
        }
    }

    public override void OnExit() { }
}