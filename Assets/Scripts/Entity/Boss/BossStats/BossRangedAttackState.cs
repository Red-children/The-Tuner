using System.Collections;
using UnityEngine;

public class BossRangedAttackState : EnemyStateBase
{
    private BossData bossData;

    public BossRangedAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        bossData = data as BossData;
        if (bossData == null)
        {
            manager.ChangeState(StateType.Chase);
            return;
        }

        controller.StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        manager.animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.3f);

        if (runtime.target != null && bossData.rangedProjectilePrefab != null)
        {
            Vector2 direction = (runtime.target.position - controller.transform.position).normalized;
            GameObject projectile = Object.Instantiate(
                bossData.rangedProjectilePrefab,
                controller.transform.position,
                Quaternion.identity
            );

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * bossData.projectileSpeed;
            }

            EnemyWeaponHit hitScript = projectile.GetComponent<EnemyWeaponHit>();
            if (hitScript != null)
            {
                hitScript.owner = controller;
                hitScript.damage = (int)bossData.Atk;
            }
        }

        yield return new WaitForSeconds(0.5f);
        manager.ChangeState(StateType.Chase);
    }

    public override void OnUpdate() { }
    public override void OnExit() { }
}
