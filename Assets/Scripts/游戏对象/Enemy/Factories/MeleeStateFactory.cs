using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//近战敌人工厂 用来创建各种状态单例
public class MeleeStateFactory : IStateFactory
{
    public IState CreateIdleState(FSM fsm) => new EnemyIdleState(fsm);
    public IState CreatePatrolState(FSM fsm) => new EnemyPatrolState(fsm);
    public IState CreateChaseState(FSM fsm) => new EnemyChaseState(fsm);
    public IState CreateApproachState(FSM fsm) => new EnemyMeleeApproachState(fsm);
    public IState CreateAttackState(FSM fsm) => new EnemyMeleeAttackState(fsm);
    public IState CreateWoundState(FSM fsm) => new EnemyWoundState(fsm);
    public IState CreateDeadState(FSM fsm) => new EnemyDeadState(fsm);
}