using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 等待状态

public class EnemyIdleState : EnemyStateBase
{
    // 等待计时器
    private float timer;

    public EnemyIdleState(FSM manager):base(manager)
    {
    }

    public override void OnStart()
    {
        Debug.Log("进入Idle状态");
        timer = 0f;
    }

    public  override void OnUpdate()
    {
        // 如果受到攻击，立即切换到受击状态
        if (parameter.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }

        timer += Time.deltaTime;
        // 如果等待时间超过设定值，切换到巡逻状态
        if (timer >= manager.CommonData.idleTime)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }
        // 如果在等待期间发现玩家，立即切换到追逐状态
        if (parameter.target != null)
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
