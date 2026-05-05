using UnityEngine;

public class BossDeadState : EnemyStateBase
{
    public BossDeadState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        manager.animator.SetTrigger("Dead");
        controller.StartCoroutine(controller.DeathCoroutine());
    }

    public override void OnUpdate() { }
    public override void OnExit() { }
}
