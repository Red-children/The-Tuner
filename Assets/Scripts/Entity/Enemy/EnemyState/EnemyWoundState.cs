using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class EnemyWoundState : EnemyStateBase
{
    public bool isAnimationPlaying = false; // 动画播放标记

    public EnemyWoundState(FSM manager) : base(manager) { }

    // 公开的重置方法，用于重复受伤时手动刷新状态


    public override void OnStart()
    {

        if(!isAnimationPlaying)
        manager.animator.SetTrigger("Wound");


        isAnimationPlaying  =true;
        Debug.Log($"[{controller.name}] 进入 Wound 状态");
        runtime.getHit = false;

        NavMeshAgent agent = controller.agent;
        if (agent != null) agent.enabled = false;

        // 执行击退位移
        if (runtime.knockbackDistance > 0f)
        {
            Vector2 dir = runtime.knockbackForce.normalized;
            float dist = runtime.knockbackDistance;
            RaycastHit2D hit = Physics2D.Raycast(manager.transform.position, dir, dist, LayerMask.GetMask("Wall"));
            Vector2 targetPos = hit.collider != null
                ? hit.point - dir * 0.1f
                : (Vector2)manager.transform.position + runtime.knockbackForce;
            manager.transform.position = targetPos;
        }
    }

    public override void OnUpdate()
    {
        if (runtime.currentHealth <= 0)
        {
            manager.ChangeState(StateType.Dead);
        }
    }

    public override void OnExit()
    {
        NavMeshAgent agent = controller.agent;
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(manager.transform.position);
        }
        
    }

    public void HandleAnimationFinished()
    {

        if (!isAnimationPlaying)
        {
            Debug.LogWarning("动画播放标记为 false，忽略本次调用");
            return;
        }
        isAnimationPlaying = false;

        if (runtime.currentHealth <= 0)
        {
            Debug.Log("受伤动画结束，切换到 Dead");
            manager.ChangeState(StateType.Dead);
            return;
        }

        if (runtime.target != null)
        {
            Debug.Log("受伤动画结束，切换到 Chase");
            manager.ChangeState(StateType.Chase);
        }
        else
        {
            Debug.Log("受伤动画结束，无目标，切换到 Idle");
            manager.ChangeState(StateType.Idle);
        }
    }
}