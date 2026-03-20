using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 巡逻状态
public class EnemyPatrolState : EnemyStateBase
{
    
    private Vector2 targetPos;
    private float minDistance = 0.1f;

    private int currentPointIndex = 0;
    private bool movingForward = true; // 用于往返

    public EnemyPatrolState(FSM manager) : base(manager)
    {
    
    }
    

    public override void OnStart()
    {
        Debug.Log("进入Patrol状态");
        if (parameter.data.patrolPoints == null || parameter.data.patrolPoints.Length == 0)
        {
            Debug.LogWarning("没有设置巡逻点，使用随机巡逻");
            GetNewRandomTarget(); // 保底逻辑
        }
        else
        {
            SetNextTarget();
        }
    }

    public override void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        if (parameter.target != null)
        {
            manager.ChangeState(StateType.Chase);
            return;
        }



        // 根据目标点方向设置 flipX
        if (targetPos.x > manager.transform.position.x)
            parameter.data.spriteRenderer.flipX = false; // 目标在右，不翻转
        else if (targetPos.x < manager.transform.position.x)
            parameter.data.spriteRenderer.flipX = true;  // 目标在左，翻转

        // 向目标点移动
        manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            targetPos,
            parameter.data.moveSpeed * Time.deltaTime);

        // 到达目标点后获取下一个点
        if (Vector2.Distance(manager.transform.position, targetPos) < minDistance)
        {
            if (parameter.data.patrolPoints.Length > 0)
                SetNextTarget();
            else
                GetNewRandomTarget();
        }
    }

    #region 巡逻点切换方法
    private void SetNextTarget()
    {
        if (parameter.data.patrolPoints.Length == 0) return;
        targetPos = parameter.data.patrolPoints[currentPointIndex].position;

        // 更新索引（往返模式）
        if (movingForward)
        {
            if (currentPointIndex == parameter.data.patrolPoints.Length - 1)
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
    #endregion
    public override void OnExit()
    {
        targetPos = Vector2.zero;
    }

    #region 旋转方法（暂时弃用）
    //private void RotateTowardsTarget()
    //{
    //    //目标点与当前敌人位置的方向向量
    //    Vector2 direction = targetPos - (Vector2)manager.transform.position;
    //    if (direction.magnitude < 0.01f) return;

    //    // 计算目标角度
    //    float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    //    // 获取当前角度（将四元数转换为欧拉角，注意我们只需要Z轴旋转）
    //    float currentAngle = manager.transform.eulerAngles.z;

    //    // 计算最短旋转角度差
    //    float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

    //    // 根据旋转速度限制每帧最大旋转角度
    //    float maxDelta = rotationSpeed * Time.deltaTime;
    //    float newAngle = currentAngle + Mathf.Clamp(angleDiff, -maxDelta, maxDelta);

    //    // 应用新旋转
    //    manager.transform.rotation = Quaternion.Euler(0, 0, newAngle);
    //}



    #endregion

    #region 随机巡逻点生成方法
    private void GetNewRandomTarget()
    {
        if (parameter.data.patrolCenter == null)
        {
            Debug.LogWarning("巡逻中心点未设置！");
            return;
        }

        // 优化随机点生成：偏向当前朝向的前方区域，减少突然掉头
        Vector2 center = parameter.data.patrolCenter.position;

        // 获取当前敌人的朝向（单位向量）
        Vector2 currentDir = manager.transform.right; // 假设敌人默认向右

        // 在朝向 ±90 度范围内随机角度，避免直接生成后方点
        float angleRange = 90f; // 可调整，越大越可能转向后方
        float randomAngle = Random.Range(-angleRange, angleRange) * Mathf.Deg2Rad;
        Vector2 randomDir = new Vector2(
            Mathf.Cos(randomAngle) * currentDir.x - Mathf.Sin(randomAngle) * currentDir.y,
            Mathf.Sin(randomAngle) * currentDir.x + Mathf.Cos(randomAngle) * currentDir.y
        ).normalized;

        // 随机半径
        float radius = Random.Range(0f, parameter.data.patrolRadius);
        Vector2 offset = randomDir * radius;

        // 最终目标点 = 巡逻中心 + 偏移
        targetPos = center + offset;
    }
    #endregion
}
#endregion