using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using System;

[Serializable]

/// <summary>
/// 远程敌人接近状态类，继承自EnemyStateBase，负责处理远程敌人在接近玩家时的行为逻辑，如调整位置以保持在攻击范围内，同时根据与玩家的距离动态调整移动方向，确保敌人能够在合理的范围内接近玩家并准备攻击，同时在状态更新中处理被击中和目标丢失的情况，确保敌人能够根据实际情况做出合理的反应，如切换到受伤状态或巡逻状态等。
/// </summary>
public class EnemyRangedApproachState : EnemyStateBase
{
    //当前的方向
    private Vector2 currentDirection;

    public EnemyRangedApproachState(FSM manager) : base(manager) { }

    public override void OnStart()
    {

        if (runtime.target != null)
            currentDirection = (runtime.target.position - manager.transform.position).normalized;
    }

    public override void OnUpdate()
    {
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

        float distance = Vector2.Distance(manager.transform.position, runtime.target.position);
        float attackRange = (data as RangedEnemyData).attackRange;

        // 简化逻辑：只要在攻击范围内就攻击
        if (distance <= attackRange)
        {
            Debug.Log($"[{controller.name}] 进入攻击范围，切换到攻击状态");
            manager.ChangeState(StateType.Attack);
            return;
        }

        // 太远，接近目标
        Vector2 toTarget = (runtime.target.position - manager.transform.position).normalized;
        manager.transform.position += (Vector3)toTarget * data.moveSpeed * Time.deltaTime;

        Debug.Log($"[{controller.name}] 接近状态: distance={distance}, attackRange={attackRange}");
    }

    public override void OnExit() { }
}