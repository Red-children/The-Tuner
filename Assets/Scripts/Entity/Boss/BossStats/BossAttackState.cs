using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossAttackState : IState
{
    private BossController controller;
    private BossFSM fsm;
    private BossRuntime runtime;
    private Coroutine attackCoroutine;

    public BossAttackState(BossController bossController)
    {
        controller = bossController;
        fsm = bossController.manager;
        runtime = bossController.runtime;
    }

    public void OnStart()
    {
        Debug.Log("Boss进入Attack状态");
        attackCoroutine = controller.StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        // 前摇
        yield return new WaitForSeconds(0.3f);

        // 伤害判定
        PerformAttack();

        // 后摇
        yield return new WaitForSeconds(0.5f);

        // 返回追逐
        fsm.ChangeState(StateType.Chase);
    }

    private void PerformAttack()
    {
        if (runtime.target == null) return;
        float distance = Vector2.Distance(controller.transform.position, runtime.target.position);
        if (distance > runtime.Data.normalAttackRange) return;

        PlayerAPI player = runtime.target.GetComponent<PlayerAPI>();
        if (player != null)
        {
            player.TakeDamage((int)runtime.Data.specialAttackDamage);
            Debug.Log($"Boss 对玩家造成 {runtime.Data.specialAttackDamage} 点伤害");
        }
    }

    public void OnExit()
    {
        if (attackCoroutine != null)
            controller.StopCoroutine(attackCoroutine);
        Debug.Log("Boss退出Attack状态");
    }

    public void OnUpdate() { }
}