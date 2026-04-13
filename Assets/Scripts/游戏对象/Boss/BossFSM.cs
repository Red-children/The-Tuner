using System.Collections.Generic;
using UnityEngine;

public class BossFSM : MonoBehaviour
{
    [SerializeReference]
    public IState currentState;

    private Dictionary<BossStateType, IState> states = new Dictionary<BossStateType, IState>();

    public BossRuntime Runtime { get; private set; }
    public BossData Data => Runtime?.Data;
    public BossController Controller { get; private set; }

    public void Initialize(BossRuntime runtime, BossController controller)
    {
        Runtime = runtime;
        Controller = controller;

        states.Add(BossStateType.Idle, new BossIdleState(Controller));
        states.Add(BossStateType.Chase, new BossChaseState(Controller));
        states.Add(BossStateType.Attack, new BossAttackState(Controller));
        states.Add(BossStateType.Wound, new BossWoundState(Controller));
        states.Add(BossStateType.Skill, new BossSkillState(Controller));
        states.Add(BossStateType.PhaseChange, new BossPhaseChangeState(Controller));
        states.Add(BossStateType.Dead, new BossDeadState(Controller)); // 启用 b.1 新增的状态

        ChangeState(BossStateType.Idle);
    }

    public void ChangeState(BossStateType newState)
    {
        if (currentState != null)
            currentState.OnExit();

        currentState = states[newState];
        currentState.OnStart();
    }

    void Update()
    {
        currentState?.OnUpdate();
    }
}