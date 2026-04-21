using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态工厂接口，定义了创建不同状态的方法，方便在FSM中根据需要创建和切换状态，同时通过接口的方式实现了状态创建的解耦，使得FSM能够灵活地使用不同的状态实现，增强了代码的可维护性和扩展性。
/// </summary>
public interface IStateFactory
{
    IState CreateIdleState(FSM fsm);
    IState CreatePatrolState(FSM fsm);
    IState CreateChaseState(FSM fsm);
    IState CreateApproachState(FSM fsm);
    IState CreateAttackState(FSM fsm);
    IState CreateWoundState(FSM fsm);
    IState CreateDeadState(FSM fsm);
}
