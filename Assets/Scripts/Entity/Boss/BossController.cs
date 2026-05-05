using UnityEngine;

public class BossController : EnemyController
{
    [Header("Boss数据")]
    public BossData bossData;

    [Header("架势系统")]
    public float currentPosture;
    public float postureRegenTimer;

    protected override void Awake()
    {
        if (bossData != null)
            data = bossData;

        base.Awake();

        currentPosture = bossData != null ? bossData.maxPosture : 100f;
        postureRegenTimer = 0f;
    }

    public override void Wound(float damage, RhythmRank rank)
    {
        if (runtime.isDead) return;

        if (runtime.getHit) return;
        runtime.getHit = true;
        runtime.currentHealth -= damage;

        postureRegenTimer = 0f;

        if (runtime.currentHealth <= 0)
        {
            ShowDamageText(transform.position, damage, rank);
            OnKilled();
            fsm?.ChangeState(StateType.Dead);
            return;
        }

        ShowDamageText(transform.position, damage, rank);
        fsm?.ChangeState(StateType.Wound);
    }

    protected override void UpdateBehavior()
    {
        if (isDead) return;

        if (bossData == null) return;

        if (postureRegenTimer < bossData.postureRegenDelay)
        {
            postureRegenTimer += Time.deltaTime;
        }
        else if (currentPosture < bossData.maxPosture)
        {
            currentPosture += bossData.postureRegenRate * Time.deltaTime;
            if (currentPosture > bossData.maxPosture)
                currentPosture = bossData.maxPosture;
        }
    }
}
