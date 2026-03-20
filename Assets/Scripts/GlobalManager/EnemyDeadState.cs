using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadState : IState
{
    private FSM manager;
    private Parameter parameter;

    public EnemyDeadState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("µĞÈËËÀÍö");
        manager.Dead();
    }

    public void OnUpdate() { }
    public void OnExit() { }
}

