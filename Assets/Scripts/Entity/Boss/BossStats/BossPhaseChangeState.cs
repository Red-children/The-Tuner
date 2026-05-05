using System.Collections;
using UnityEngine;

public class BossPhaseChangeState : EnemyStateBase
{
    private BossData bossData;
    private int nextPhase;

    public BossPhaseChangeState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        bossData = data as BossData;
        nextPhase = GetCurrentPhase() + 1;

        manager.animator.SetTrigger("PhaseChange");
        controller.StartCoroutine(PhaseChangeRoutine());
    }

    private int GetCurrentPhase()
    {
        float healthPercent = runtime.currentHealth / bossData.health;
        if (healthPercent > 0.66f) return 1;
        if (healthPercent > 0.33f) return 2;
        return 3;
    }

    private IEnumerator PhaseChangeRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (bossData != null)
        {
            float phaseMult = 1f + (nextPhase - 1) * 0.25f;
            runtime.currentMoveSpeed = bossData.moveSpeed * phaseMult;
            runtime.currentChaseSpeed = bossData.chaseSpeed * phaseMult;
        }

        if (runtime.target != null)
            manager.ChangeState(StateType.Chase);
        else
            manager.ChangeState(StateType.Idle);
    }

    public override void OnUpdate() { }
    public override void OnExit() { }
}
