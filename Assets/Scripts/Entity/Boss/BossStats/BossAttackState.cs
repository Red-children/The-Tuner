using System.Collections;
using UnityEngine;

public class BossAttackState : EnemyStateBase
{
    private BossData bossData;
    private Coroutine attackCoroutine;

    public BossAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        bossData = data as BossData;
        manager.animator.SetTrigger("Attack");
        attackCoroutine = controller.StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        PerformAttack();
        yield return new WaitForSeconds(0.5f);
        manager.ChangeState(StateType.Chase);
    }

    private void PerformAttack()
    {
        if (runtime.target == null || bossData == null) return;

        float distance = Vector2.Distance(controller.transform.position, runtime.target.position);
        if (distance > bossData.normalAttackRange) return;

        PlayerAPI player = runtime.target.GetComponent<PlayerAPI>();
        if (player != null)
        {
            player.TakeDamage((int)bossData.specialAttackDamage);
        }
    }

    public override void OnExit()
    {
        if (attackCoroutine != null)
            controller.StopCoroutine(attackCoroutine);
    }

    public override void OnUpdate() { }
}
