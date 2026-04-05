//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//public class RangedStateFactory : IStateFactory
//{
//    public IState CreateIdleState(FSM fsm) => new EnemyIdleState(fsm);
//    public IState CreatePatrolState(FSM fsm) => new EnemyPatrolState(fsm);
//    public IState CreateChaseState(FSM fsm) => new EnemyChaseState(fsm);
//    public IState CreateApproachState(FSM fsm) => new EnemyRangedApproachState(fsm);
//    public IState CreateAttackState(FSM fsm) => new EnemyRangedAttackState(fsm);
//    public IState CreateWoundState(FSM fsm) => new EnemyWoundState(fsm);
//    public IState CreateDeadState(FSM fsm) => new EnemyDeadState(fsm);
//}