using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region �ȴ�״̬

public class EnemyIdleState : EnemyStateBase
{
    // 计时器
    private float timer;

    public EnemyIdleState(FSM manager) : base(manager)
    {
    }

    public override void OnStart()
    {
        Debug.Log("敌人进入空闲状态");
        manager.animator.SetTrigger("Idle");
        timer = 0f;
    }

    public override void OnUpdate()
    {
        //  如果敌人受到攻击，立即切换到受伤状态
        if (runtime.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        timer += Time.deltaTime;
        // 如果等待时间超过设定的空闲时间，切换到巡逻状态
        if (timer >= data.idleTime)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }
        // 如果在空闲状态下发现目标，立即切换到追逐状态
        if (runtime.target != null)
        {
            manager.ChangeState(StateType.Chase);
        }
    }

    public override void OnExit()
    {
        timer = 0f;
    }
}
#endregion
