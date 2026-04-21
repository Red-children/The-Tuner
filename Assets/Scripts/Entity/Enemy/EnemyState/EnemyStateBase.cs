using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人状态基类，定义了敌人状态的基本结构和行为，所有具体的敌人状态都继承自这个基类，实现各自的状态逻辑，方便在FSM中进行状态切换和管理。
/// </summary>
public abstract class EnemyStateBase : IState
{
    //状态管理器引用
    protected FSM manager;
    // 运行时数据引用
    protected EnemyRuntime runtime => manager.Runtime;
    // 敌人数据引用，方便在状态中访问敌人的属性和配置
    protected EnemyData data => manager.Data;  
    protected EnemyController controller => manager.Controller;  // 控制器引用

    public EnemyStateBase(FSM manager)
    {
        this.manager = manager;
    }

    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}
