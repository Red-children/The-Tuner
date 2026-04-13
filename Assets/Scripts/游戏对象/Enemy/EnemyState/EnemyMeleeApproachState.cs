using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近战敌人接近状态
/// </summary>
public class EnemyMeleeApproachState : EnemyStateBase
{
    private Vector2 currentDirection;
    private float directionChangeTimer;
    private float directionChangeInterval = 0.5f;
    private float maxTurnAnglePerSec = 120f;

    public EnemyMeleeApproachState(FSM manager) : base(manager) { }

    public override void OnStart()
    {

        manager.animator.SetTrigger("Move");
         // 初始化朝向，如果有目标则朝向目标，否则保持当前朝向
        if (runtime.target != null)
            currentDirection = (runtime.target.position - manager.transform.position).normalized;
        directionChangeTimer = directionChangeInterval;
    }

    public override void OnUpdate()
    {
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

        // 处理数据转换
        MeleeEnemyData meleeData = data as MeleeEnemyData;
        if (meleeData == null) return;
        // 计算与目标的距离和方向
        float distance = Vector2.Distance(manager.transform.position, runtime.target.position);
        Vector2 toTarget = (runtime.target.position - manager.transform.position).normalized;

        // 如果在攻击范围内，切换到攻击状态
        if (distance <= meleeData.attackRange)
        {
            manager.ChangeState(StateType.Attack);
            return;
        }

        // 定期更新朝向，避免过于频繁的转向计算
        RaycastHit2D hit = Physics2D.Raycast(manager.transform.position, currentDirection, 1.5f, LayerMask.GetMask("Wall"));
        Vector2 desiredDirection = hit.collider != null ? (toTarget + (Vector2)manager.transform.right).normalized : toTarget;

        // 平滑转向
        float angleDelta = Vector2.SignedAngle(currentDirection, desiredDirection);
        float maxDelta = maxTurnAnglePerSec * Time.deltaTime;
        float newAngle = Mathf.MoveTowardsAngle(
            Vector2.SignedAngle(Vector2.right, currentDirection),
            Vector2.SignedAngle(Vector2.right, desiredDirection),
            maxDelta
        );
        currentDirection = Quaternion.Euler(0, 0, newAngle) * Vector2.right;

        // 移动敌人
        manager.transform.position += (Vector3)currentDirection * data.moveSpeed * Time.deltaTime;

        // 更新朝向
       controller.FaceTarget(runtime.target.position);
    }

    public override void OnExit() { }
}