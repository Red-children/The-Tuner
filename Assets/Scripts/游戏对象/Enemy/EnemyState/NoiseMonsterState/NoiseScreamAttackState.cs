
using UnityEngine;
using System.Collections;

[System.Serializable]
public class NoiseScreamAttackState : EnemyStateBase
{
    private NoiseMonsterData noiseData;

    public NoiseScreamAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        Debug.Log("进入嘶吼攻击状态");

        noiseData = data as NoiseMonsterData;
        if (noiseData == null)
        {
            manager.ChangeState(StateType.Chase);
            return;
        }

        controller.StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        // 播放嘶吼动画
        manager.animator.SetTrigger("Scream");
        // 等待动画关键帧（可用动画事件替代，这里用协程演示）
        yield return new WaitForSeconds(0.3f);
        PerformScream();
        yield return new WaitForSeconds(0.5f);
        manager.ChangeState(StateType.Chase);
    }

    private void PerformScream()
    {
        // 范围伤害（扇形可后续细化）
        Collider2D[] hits = Physics2D.OverlapCircleAll(manager.transform.position, 
            noiseData.noiseScreamRange, noiseData.targetLayer);
        foreach (var hit in hits)
        {
            PlayerAPI player = hit.GetComponent<PlayerAPI>();
            Debug.Log($"嘶吼攻击检测到对象: {hit.name}");
            player?.TakeDamage(noiseData.noiseScreamDamage);
            Debug.Log($"嘶吼攻击造成伤害: {noiseData.noiseScreamDamage} 给 {hit.name}");
        }
        // 播放特效
        if (noiseData.noiseScreamEffectPrefab)
            Object.Instantiate(noiseData.noiseScreamEffectPrefab, manager.transform.position, Quaternion.identity);
    }

    public override void OnExit()
    {
        // 确保动画重置
    }
}