using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyStateBase
{
    public EnemyChaseState(FSM manager) : base(manager) { }

    public override void OnStart()
    {

    }

    public override void OnUpdate()
    {
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

        // 面朝玩家（使用 controller 中的 spriteRenderer）
        bool flip = runtime.target.position.x < manager.transform.position.x;
        controller.spriteRenderer.flipX = flip;

        // 移动（速度从 data 读取）
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            runtime.target.position,
            data.chaseSpeed * Time.deltaTime);

        // 根据敌人实际类型判断是否进入接近状态
        if (data is RangedEnemyData rangedData)
        {
            float distance = Vector2.Distance(manager.transform.position, runtime.target.position);
            if (distance <= rangedData.attackRange)
            {
                manager.ChangeState(StateType.Approach);
            }
        }
        else if (data is MeleeEnemyData meleeData)
        {
            // 近战攻击范围检测（使用 controller 中的攻击点计算方法）
            if (Physics2D.OverlapCircle(controller.GetAttackWorldPos(), meleeData.attackRange, meleeData.targetLayer))
            {
                manager.ChangeState(StateType.Approach);
            }
        }
        // 如果有自爆等其他类型，可以继续添加
    }

    public override void OnExit() { }
}