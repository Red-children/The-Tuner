using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

/// <summary>
/// 近战敌人接近状态
/// </summary>
/// 
[Serializable]

public class EnemyMeleeApproachState : EnemyStateBase
{
    //当前的方向
    private Vector2 currentDirection;
    private float maxTurnAnglePerSec = 120f;

    public EnemyMeleeApproachState(FSM manager) : base(manager) { }

    public override void OnStart()
    {

        manager.animator.SetTrigger("Move");
         // 初始化朝向，如果有目标则朝向目标，否则保持当前朝向
        if (runtime.target != null)
            currentDirection = (runtime.target.position - manager.transform.position).normalized;
    }

   public override void OnUpdate()
{
    if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
    if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

    controller.FaceTarget(runtime.target.position);

    // 视线检测
    if (!controller.CanSeePlayer())
    {
        runtime.isPursuing = false;
        runtime.ignoreTargetUntilTime = Time.time + 3f;
        manager.ChangeState(StateType.Patrol);
        return;
    }

    // 用攻击点的扇形检测来判断是否进入攻击状态
    MeleeEnemyData meleeData = data as MeleeEnemyData;
    if (meleeData == null) { manager.ChangeState(StateType.Patrol); return; }

    Vector2 attackWorldPos = controller.GetAttackWorldPos();
    Vector2 toPlayer = (Vector2)runtime.target.position - attackWorldPos;
    float distanceToPlayer = toPlayer.magnitude;

    // 距离检测
    if (distanceToPlayer <= meleeData.attackRange)
    {
        // 角度检测：玩家是否在攻击点的前方锥形内
        Vector2 forward = controller.isFacingRight ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(forward, toPlayer.normalized);
        if (angle <= meleeData.attackAngle * 0.5f)
        {
            manager.ChangeState(StateType.Attack);
            return;
        }
    }

    // 如果玩家跑远了，回到追击状态
    float distanceToTarget = Vector2.Distance(manager.transform.position, runtime.target.position);
    if (distanceToTarget > meleeData.approachDistance * 1.2f) // 稍微给点余量
    {
        manager.ChangeState(StateType.Chase);
        return;
    }

    // 在接近状态下，可以做战术移动（侧移、保持距离等）
    // 也可以简单保持当前位置，面向玩家
    // 这里可以加上最小距离控制，防止贴脸推人
    float stoppingDistance = meleeData.stopMinRange;
    if (distanceToTarget > stoppingDistance)
    {
        // 缓慢靠近
        NavMeshAgent agent = controller.agent;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(runtime.target.position);
            agent.speed = data.moveSpeed * 0.5f; // 接近时速度放慢
        }
    }
    else
    {
        // 太近了，停下
        NavMeshAgent agent = controller.agent;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
        manager.ChangeState (StateType.Attack);
        
    }
}

    public override void OnExit() { }
}