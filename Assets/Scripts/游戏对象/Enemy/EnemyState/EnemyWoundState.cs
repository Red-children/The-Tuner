using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#region 敌人受伤方法

public class EnemyWoundState : EnemyStateBase
{

    private float timer;        // 受击硬直计时器

    public float finallyDamage;//最终伤害值 在Wound方法中计算并赋值

    public EnemyWoundState(FSM manager) : base(manager)
    {
    }

    public override void OnStart()
    {
        Debug.Log("进入Wound状态");
        runtime.getHit = false;

            NavMeshAgent agent = controller.agent;
    if (agent != null) agent.enabled = false; // 暂时禁用，避免拉扯

        // 执行击退
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

        timer = 0f;

    }

    public override void OnUpdate()
    {
        if (runtime.currentHealth <= 0)
        {
            manager.ChangeState(StateType.Dead);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= 0.5f) // 受击硬直时间
        {
            if (runtime.target != null)
                manager.ChangeState(StateType.Chase);
            else
                manager.ChangeState(StateType.Patrol);
        }
    }

   public override void OnExit()
{
    NavMeshAgent agent = controller.agent;
    if (agent != null)
    {
        agent.enabled = true;
        // 可选：将agent的位置同步到当前位置
        agent.Warp(manager.transform.position);
    }
}



}

#endregion