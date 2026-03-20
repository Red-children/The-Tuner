using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeApproachState : EnemyStateBase
{
    private float approachTime = 2f;
    private float timer;
    private Vector2 currentDirection;
    private float directionChangeTimer;
    private float directionChangeInterval = 0.5f; // 每0.5秒才考虑变向

    // 转向限制
    private float maxTurnAnglePerSec = 120f; // 每秒最多转120度

    public EnemyMeleeApproachState(FSM manager):base(manager)
    {
       
    }

    public override void OnStart()
    {
        Debug.Log("进入Approach状态");
        timer = 0f;
        parameter.animator.Play("Attack");
        // 初始方向朝向玩家
        if (parameter.target != null)
        {
            currentDirection = (parameter.target.position - manager.transform.position).normalized;
        }
        directionChangeTimer = directionChangeInterval;
    }

    public override void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null) { manager.ChangeState(StateType.Patrol); return; }

        // 1. 计算期望方向（朝向玩家）
        Vector2 toPlayer = (parameter.target.position - manager.transform.position).normalized;

        // 2. 射线检测避障（简单版）
        RaycastHit2D hit = Physics2D.Raycast(manager.transform.position, currentDirection, 1.5f, LayerMask.GetMask("Wall"));
        if (hit.collider != null)
        {
            // 如果前方有墙，强制转向（比如向左转）
            toPlayer = Quaternion.Euler(0, 0, 45) * toPlayer;
        }

        // 3. 转向限制：不能直接从当前方向突变到期望方向
        float angleBetween = Vector2.SignedAngle(currentDirection, toPlayer);
        float maxDelta = maxTurnAnglePerSec * Time.deltaTime;
        float newAngle = Mathf.MoveTowardsAngle(
            Vector2.SignedAngle(Vector2.right, currentDirection),
            Vector2.SignedAngle(Vector2.right, toPlayer),
            maxDelta
        );
        currentDirection = Quaternion.Euler(0, 0, newAngle) * Vector2.right;

        // 4. 移动
        manager.transform.position += (Vector3)currentDirection * parameter.moveSpeed * Time.deltaTime;

        // 5. 面朝玩家（翻转）
        if (parameter.target.position.x > manager.transform.position.x)
            parameter.spriteRenderer.flipX = false;
        else
            parameter.spriteRenderer.flipX = true;

        // 6. 计时切换
        timer += Time.deltaTime;
        if (timer >= approachTime)
        {
            manager.ChangeState(StateType.Attack);
        }
    }

    public override void OnExit() { }
}