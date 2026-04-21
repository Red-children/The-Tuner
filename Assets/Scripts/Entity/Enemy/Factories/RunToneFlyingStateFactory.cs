using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunToneFlyingStateFactory : IStateFactory
{
    public IState CreateIdleState(FSM fsm) => new RunToneFlyingIdleState(fsm);
    public IState CreatePatrolState(FSM fsm) => new RunToneFlyingIdleState(fsm); // 不需要巡逻
    public IState CreateChaseState(FSM fsm) => new RunToneFlyingChaseState(fsm); // 实际是冲撞准备
    public IState CreateApproachState(FSM fsm) => new RunToneFlyingIdleState(fsm);
    public IState CreateAttackState(FSM fsm) => new RunToneFlyingChargeState(fsm); // 冲撞
    public IState CreateWoundState(FSM fsm) => new EnemyWoundState(fsm);
    public IState CreateDeadState(FSM fsm) => new EnemyDeadState(fsm);
}