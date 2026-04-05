//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EnemyMeleeApproachState : EnemyStateBase
//{
//    private Vector2 currentDirection;
//    private float directionChangeTimer;
//    private float directionChangeInterval = 0.5f;
//    private float maxTurnAnglePerSec = 120f;

//    public EnemyMeleeApproachState(FSM manager) : base(manager) { }

//    public override void OnStart()
//    {
        
//        if (runtime.target != null)
//            currentDirection = (runtime.target.position - manager.transform.position).normalized;
//        directionChangeTimer = directionChangeInterval;
//    }

//    public override void OnUpdate()
//    {
//        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
//        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

//        // 获取攻击范围数据
//        MeleeEnemyData meleeData = data as MeleeEnemyData;
//        if (meleeData == null) return;

//        float distance = Vector2.Distance(manager.transform.position, runtime.target.position);
//        Vector2 toTarget = (runtime.target.position - manager.transform.position).normalized;

//        // 如果已经在攻击范围内，立即攻击
//        if (distance <= meleeData.attackRange)
//        {
//            manager.ChangeState(StateType.Attack);
//            return;
//        }

//        // 否则继续接近玩家
//        // 避障射线检测（简单版）
//        RaycastHit2D hit = Physics2D.Raycast(manager.transform.position, currentDirection, 1.5f, LayerMask.GetMask("Wall"));
//        Vector2 desiredDirection = hit.collider != null ? (toTarget + (Vector2)manager.transform.right).normalized : toTarget;

//        // 转向限制
//        float angleDelta = Vector2.SignedAngle(currentDirection, desiredDirection);
//        float maxDelta = maxTurnAnglePerSec * Time.deltaTime;
//        float newAngle = Mathf.MoveTowardsAngle(
//            Vector2.SignedAngle(Vector2.right, currentDirection),
//            Vector2.SignedAngle(Vector2.right, desiredDirection),
//            maxDelta
//        );
//        currentDirection = Quaternion.Euler(0, 0, newAngle) * Vector2.right;

//        // 移动
//        manager.transform.position += (Vector3)currentDirection * data.moveSpeed * Time.deltaTime;

//        // 面朝玩家
//        controller.spriteRenderer.flipX = runtime.target.position.x < manager.transform.position.x;
//    }

//    public override void OnExit() { }
//}