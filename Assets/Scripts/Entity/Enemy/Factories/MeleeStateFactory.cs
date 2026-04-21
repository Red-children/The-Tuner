using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近战敌人状态工厂类，实现了IStateFactory接口，负责创建近战敌人各个状态的实例，如Idle、Patrol、Chase、Approach、Attack、Wound和Dead等状态，确保在FSM中能够根据需要创建和切换近战敌人的不同状态，同时通过工厂模式实现了状态创建的集中管理，增强了代码的可维护性和扩展性。
/// </summary>
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