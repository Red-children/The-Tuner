using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public abstract class EnemyStateBase : IState
{
    protected FSM manager;
    protected Parameter parameter;

    public EnemyStateBase(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}