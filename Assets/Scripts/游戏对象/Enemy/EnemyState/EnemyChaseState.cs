using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人追击状态类，负责处理敌人在追逐玩家时的行为逻辑，包括朝向目标、移动以及判断是否进入攻击范围等，根据敌人类型（近战或远程）进行不同的处理。
/// </summary>

public class EnemyChaseState : EnemyStateBase
{
    public EnemyChaseState(FSM manager) : base(manager) { }

    public override void OnStart()
    {

    }

    public override void OnUpdate()
    {
        // 如果受到攻击，立即切换到受伤状态
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }

        // 如果目标丢失，切换回巡逻状态
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

        // 在追逐状态下，敌人会朝向目标并移动
        bool flip = runtime.target.position.x < manager.transform.position.x;
        controller.spriteRenderer.flipX = flip;

        // 移动敌人朝向目标
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position, // 当前敌人位置
            runtime.target.position,    // 目标位置
            data.chaseSpeed * Time.deltaTime // 追逐速度乘以时间增量，确保帧率独立的移动
            );

        // 根据敌人类型判断是否进入攻击范围
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
            // 近战敌人使用OverlapCircle检测是否进入攻击范围，确保敌人能够正确地判断何时可以攻击玩家
            if (Physics2D.OverlapCircle(controller.GetAttackWorldPos(), meleeData.attackRange, meleeData.targetLayer))
            {
                manager.ChangeState(StateType.Approach);
            }
        }
    }

    public override void OnExit() { }
}