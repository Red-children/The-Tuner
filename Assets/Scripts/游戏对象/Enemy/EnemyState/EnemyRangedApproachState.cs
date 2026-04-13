using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
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
        float attackRange = (data as RangedEnemyData).attackRange;  // 获取远程敌人的攻击范围

        // 定义一个理想的距离范围，敌人会尝试保持在这个范围内进行接近，确保敌人能够在合理的范围内接近玩家并准备攻击，同时根据与玩家的距离动态调整移动方向，形成更智能的接近行为
        float minDesired = attackRange * 0.6f;
        float maxDesired = attackRange * 0.9f;

        Vector2 toTarget = (runtime.target.position - manager.transform.position).normalized;

        // 根据与目标的距离调整移动方向，确保敌人能够根据与玩家的距离动态调整移动方向，形成更智能的接近行为
        if (distance < minDesired)
        {
            // ̫间距过近，远离目标
            currentDirection = -toTarget;
        }
        else if (distance > maxDesired)
        {
            // ̫间距过远，接近目标
            currentDirection = toTarget;
        }
        else
        {
            // 间距合适，进入攻击状态
            currentDirection = Vector2.zero; // 停止移动，保持在当前距离
            manager.ChangeState(StateType.Attack);
            return;
        }

        // 移动敌人朝向目标
        manager.transform.position += (Vector3)currentDirection * data.moveSpeed * Time.deltaTime;

        // 根据目标位置调整敌人朝向，确保敌人始终面向玩家，形成更自然的行为表现
        if (runtime.target.position.x > manager.transform.position.x)
            controller.spriteRenderer.flipX = false;
        else
            controller.spriteRenderer.flipX = true;
    }

    public override void OnExit() { }
}