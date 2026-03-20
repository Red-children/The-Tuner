using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyStateBase
{
   
    public override void OnStart()
    {
        Debug.Log("进入Chase状态");
    }

    public EnemyChaseState(FSM manager) : base(manager) { }

    public override void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null) { manager.ChangeState(StateType.Patrol); return; }

        // 根据玩家位置设置 flipX
        if (parameter.target.position.x > manager.transform.position.x)
            parameter.spriteRenderer.flipX = false;
        else
            parameter.spriteRenderer.flipX = true;

        // 向玩家移动
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            parameter.target.position,
            parameter.chaseSpeed * Time.deltaTime);

        // 检查是否进入攻击范围（区分近战和远程）
        if (parameter.enemyType == EnemyType.Ranged)
        {
            float distance = Vector2.Distance(manager.transform.position, parameter.target.position);
            if (distance <= parameter.attackRange)
            {
                manager.ChangeState(StateType.Approach);
            }
        }
        else // 近战
        {
            if (Physics2D.OverlapCircle(manager.GetAttackWorldPos(), parameter.attackRange, parameter.targetLayer))
            {
                manager.ChangeState(StateType.Approach);
            }
        }
    }




    public override void OnExit() { }
}
