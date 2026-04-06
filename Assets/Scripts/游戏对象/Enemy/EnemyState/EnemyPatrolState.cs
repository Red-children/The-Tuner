using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人巡逻状态类，继承自EnemyStateBase，负责处理敌人在巡逻状态下的行为逻辑，包括在预设的巡逻点之间移动、随机生成新的巡逻目标等功能，确保敌人在没有玩家接近时能够进行合理的巡逻行为。
/// </summary>
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

        if (runtime.target != null)
        {
            manager.ChangeState(StateType.Chase);
            return;
        }



        // 根据目标点的位置调整敌人的朝向，确保敌人始终面向移动的方向
        if (targetPos.x > manager.transform.position.x)
            controller.spriteRenderer.flipX = false; //目标点在右边，面向右边
        else if (targetPos.x < manager.transform.position.x)
            controller.spriteRenderer.flipX = true;  // 目标点在左边，面向左边

        //朝向目标点移动
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            targetPos,
            data.moveSpeed * Time.deltaTime);

        // 
        if (Vector2.Distance(manager.transform.position, targetPos) < minDistance)
        {
            if (controller.patrolPoints.Length > 0)
                SetNextTarget();
            else
                GetNewRandomTarget();
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
        if (controller.patrolCenter == null)
        {Debug.LogWarning("没有设置巡逻中心，无法生成随机巡逻目标");
            return;
        }

        // 得到巡逻中心位置，确保随机目标是在巡逻中心附近生成的，形成合理的巡逻范围
        Vector2 center = controller.patrolCenter.position;

        // 得到当前敌人朝向，作为生成随机方向的基础，确保随机目标的生成是基于敌人当前的朝向，形成更自然的巡逻行为
        Vector2 currentDir = manager.transform.right; 

        // 生成一个随机方向，基于当前朝向进行一定范围内的随机偏转，确保敌人能够在巡逻中心附近的不同方向上生成随机目标，形成更丰富的巡逻路径
        float angleRange = 90f; 
        // 生成一个随机角度，范围在[-angleRange, angleRange]之间，确保随机目标的生成是在当前朝向的基础上进行一定范围内的随机偏转，形成更自然的巡逻行为
        float randomAngle = Random.Range(-angleRange, angleRange) * Mathf.Deg2Rad;
        // 计算随机方向，使用旋转矩阵将当前朝向进行旋转，得到一个新的随机方向，确保随机目标的生成是基于敌人当前的朝向进行一定范围内的随机偏转，形成更自然的巡逻行为
        Vector2 randomDir = new Vector2(
            Mathf.Cos(randomAngle) * currentDir.x - Mathf.Sin(randomAngle) * currentDir.y,
            Mathf.Sin(randomAngle) * currentDir.x + Mathf.Cos(randomAngle) * currentDir.y
        ).normalized;

        // 生成一个随机距离，范围在[0, data.patrolRadius]之间，确保随机目标的生成是在巡逻中心附近的合理范围内，形成合理的巡逻行为
        float radius = Random.Range(0f, data.patrolRadius);
        Vector2 offset = randomDir * radius;

        // 计算最终的随机目标位置，确保随机目标是在巡逻中心附近的合理位置，形成合理的巡逻行为
        targetPos = center + offset;
    }
  
}
