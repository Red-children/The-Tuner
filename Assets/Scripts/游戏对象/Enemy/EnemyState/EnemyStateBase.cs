using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class EnemyStateBase : IState
{
    protected FSM manager;
    protected EnemyRuntime runtime => manager.Runtime;
    protected EnemyData data => manager.Data;  // 修复类型错误
    protected EnemyController controller => manager.Controller;  // 控制器引用

    public EnemyStateBase(FSM manager)
    {
        this.manager = manager;
    }

    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}
