using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadState : EnemyStateBase
{


    public EnemyDeadState(FSM manager) : base(manager)
    {
    }

    public override void OnStart()
    {
        Debug.Log("敌人进入死亡状态");
        controller.Dead();
    }

    public override void OnUpdate() { }
    public override void OnExit() { }
}

