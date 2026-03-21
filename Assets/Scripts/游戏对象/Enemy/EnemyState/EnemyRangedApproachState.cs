using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class EnemyRangedApproachState : EnemyStateBase
{
    private Vector2 currentDirection;

    public EnemyRangedApproachState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        Debug.Log("进入远程接近状态");
        if (runtime.target != null)
            currentDirection = (runtime.target.position - manager.transform.position).normalized;
    }

    public override void OnUpdate()
    {
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

        float distance = Vector2.Distance(manager.transform.position, runtime.target.position);
        float attackRange = (data as RangedEnemyData).attackRange;  // 从 manager 获取远程攻击范围

        // 定义理想距离范围：攻击范围的 60%~90%
        float minDesired = attackRange * 0.6f;
        float maxDesired = attackRange * 0.9f;

        Vector2 toTarget = (runtime.target.position - manager.transform.position).normalized;

        // 判断距离是否合适
        if (distance < minDesired)
        {
            // 太近，远离玩家
            currentDirection = -toTarget;
        }
        else if (distance > maxDesired)
        {
            // 太远，靠近玩家
            currentDirection = toTarget;
        }
        else
        {
            // 距离合适，立即切换到攻击状态
            manager.ChangeState(StateType.Attack);
            return;
        }

        // 执行移动（使用 manager 中的移动速度）
        manager.transform.position += (Vector3)currentDirection * data.moveSpeed * Time.deltaTime;

        // 面朝玩家（翻转）使用 manager 中的 SpriteRenderer
        if (runtime.target.position.x > manager.transform.position.x)
            controller.spriteRenderer.flipX = false;
        else
            controller.spriteRenderer.flipX = true;
    }

    public override void OnExit() { }
}