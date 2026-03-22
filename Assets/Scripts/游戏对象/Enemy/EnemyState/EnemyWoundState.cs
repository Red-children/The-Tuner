using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 敌人受伤方法

public class EnemyWoundState : EnemyStateBase
{
  
    private float timer;        // 受击硬直计时器

    public float finallyDamage;//最终伤害值 在Wound方法中计算并赋值

    public EnemyWoundState(FSM manager):base(manager)
    { 
    }

    public override void OnStart()
    {
        data.health -= finallyDamage; // 直接在这里扣血，确保状态切换时已经计算好最终伤害
        Debug.Log("进入Wound状态");
        runtime.getHit = false;
        
        timer = 0f;

    }

    public  override void OnUpdate()
    {
        if (data.health <= 0)
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

    public override    void OnExit() { }



}

#endregion