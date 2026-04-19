using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敌人追击状态类，负责处理敌人在追逐玩家时的行为逻辑，包括朝向目标、移动以及判断是否进入攻击范围等，根据敌人类型（近战或远程）进行不同的处理。
/// </summary>

[Serializable]
public class EnemyChaseState : EnemyStateBase
{
    public EnemyChaseState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        manager.animator.SetTrigger("Move");
        controller.agent.speed = data.chaseSpeed;
    }

    public override void OnUpdate()
    {


        // 如果受到攻击，立即切换到受伤状态
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }

        // 如果目标丢失，切换回巡逻状态
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

        float maxChase = 0f;


        if (data is MeleeEnemyData mData)
            maxChase = mData.maxChaseDistance;
        // 如果是其他类型（如跑调飞虫），可以给一个默认值或跳过检查
        else
            maxChase = float.MaxValue; // 无限追击

        //测试技能区域 *************************************************

        //先让敌人转向
        controller.FaceTarget(runtime.target.position);

        //测试视线锥效果的区域 ****************************
        // 在移动之前加入视觉确认
        // 视野检查
        if (!controller.CanSeePlayer())
        {
            Debug.Log("视线锥检测失败，不允许进入追击状态");
            runtime.isPursuing = false;
            runtime.ignoreTargetUntilTime = Time.time + 3f;
            manager.ChangeState(StateType.Patrol);
            return;
        }
        //测试视线锥效果的区域 ****************************



        // 在 EnemyChaseState.OnUpdate 中，距离检查之后、攻击范围判断之前
        if (data is NoiseMonsterData noiseData)
        {
            float dist = Vector2.Distance(manager.transform.position, runtime.target.position);
            if (dist <= noiseData.noiseScreamRange && Time.time >= runtime.lastNoiseScreamTime + noiseData.noiseScreamCooldown)
            {
                runtime.lastNoiseScreamTime = Time.time;
                manager.ChangeState(StateType.NoiseScreamWarning);
                return;
            }
        }
        //测试技能区域 *************************************************



        //if (distanceToTarget > maxChase)
        //{
        //    runtime.target = null;
        //   manager.ChangeState(StateType.Patrol);
        //    return;
        //}


        // 先让敌人转向（视觉朝向）
        controller.FaceTarget(runtime.target.position);

        // 移动：交给NavMeshAgent
        NavMeshAgent agent = controller.agent;
        if (agent != null && agent.isOnNavMesh)
        {
            //让敌人追击玩家
            agent.SetDestination(runtime.target.position);

            // 可选：根据节奏调整速度（保留你的节奏系统）
            float beatProgress = (float)RhythmManager.Instance.BeatProgress;
            float speedMultiplier = Mathf.Sin(beatProgress * Mathf.PI);
            agent.speed = runtime.currentChaseSpeed * Mathf.Lerp(0.6f, 1.4f, speedMultiplier);
        }

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