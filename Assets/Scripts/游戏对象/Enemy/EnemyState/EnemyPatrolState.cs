using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敌人巡逻状态类，继承自EnemyStateBase，负责处理敌人在巡逻状态下的行为逻辑，包括在预设的巡逻点之间移动、随机生成新的巡逻目标等功能，确保敌人在没有玩家接近时能够进行合理的巡逻行为。
/// </summary>
/// 
[System.Serializable]
public class EnemyPatrolState : EnemyStateBase
{

    private Vector2 targetPos;
    private float minDistance = 0.1f;

    private int currentPointIndex = 0;
    private bool movingForward = true; // 是否正在向前巡逻

    public EnemyPatrolState(FSM manager) : base(manager)
    {
    }


    public override void OnStart()
    {
        Debug.Log("敌人进入巡逻状态");
        if (controller.patrolPoints == null || controller.patrolPoints.Length == 0)
        {
            Debug.LogWarning("没有设置巡逻点，使用随机巡逻");
            GetNewRandomTarget(); // 生成新的随机目标
        }
        else
        {
            SetNextTarget();
        }
    }

    public override void OnUpdate()
    {
        if (runtime.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        if (runtime.target != null && runtime.isPursuing)
        {
            manager.ChangeState(StateType.Chase);
            return;
        }

        controller.FaceTarget(targetPos);

        if (runtime.target != null && !runtime.isPursuing)
        {
            // 冷却结束且目标仍在，重新激活追击
            if (Time.time >= runtime.ignoreTargetUntilTime)
            {
                runtime.isPursuing = true;
                manager.ChangeState(StateType.Chase);
                return;
            }
        }



        controller.FaceTarget(targetPos);
        //读取引用网格
        NavMeshAgent agent = controller.agent;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(targetPos);
            agent.speed = data.moveSpeed;
        }

        // 检查是否到达（使用agent.remainingDistance）
        if (agent != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("到达巡逻点，进入待机");
            manager.ChangeState(StateType.Idle);
            return;
        }




    }

    /// <summary>
    /// 得到下一个目标点，按照预设的巡逻点顺序进行巡逻，当到达最后一个巡逻点时会反向巡逻，确保敌人能够在预设的巡逻点之间合理地移动，形成一个闭环的巡逻路径。
    /// </summary>
    private void SetNextTarget()
    {
        if (controller.patrolPoints.Length == 0) return;
        targetPos = controller.patrolPoints[currentPointIndex].position;


        if (movingForward)
        {
            // 如果当前巡逻点是最后一个，切换方向，否则继续向前
            if (currentPointIndex == controller.patrolPoints.Length - 1)
                movingForward = false;
            else
                currentPointIndex++;
        }
        else
        {
            if (currentPointIndex == 0)
                movingForward = true;
            else
                currentPointIndex--;
        }
    }

    public override void OnExit()
    {
        targetPos = Vector2.zero;
    }



    private void GetNewRandomTarget()
    {
        // === 原来的中心点计算逻辑（保持不变） ===
        Vector2 center;
        if (controller.patrolCenter != null)
        {
            center = controller.patrolCenter.position;
        }
        else if (controller.transform.parent != null)
        {
            center = controller.transform.parent.position;
            Debug.Log($"[{controller.name}] 使用父物体位置作为巡逻中心: {center}");
        }
        else
        {
            center = controller.transform.position;
            Debug.LogError($"[{controller.name}] 既没有 patrolCenter 也没有父物体！巡逻中心回退到自身，会导致漂移！");
        }

        float radius = data.patrolRadius;

        // === 新：使用导航网格生成有效点 ===
        targetPos = GetRandomNavMeshPoint(center, radius);
    }

    /// <summary>
    /// 在导航网格上生成一个随机有效点
    /// </summary>
    public Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = center + (Vector3)(Random.insideUnitCircle * radius);
            randomPos.z = 0;
            if (NavMesh.SamplePosition(randomPos, out UnityEngine.AI.NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center; // 没找到就返回中心点
    }

}