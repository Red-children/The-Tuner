using UnityEngine;

public class BossWoundState : EnemyStateBase
{
    private BossData bossData;
    private float timer;
    private float stunDuration = 0.5f;

    public BossWoundState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        bossData = data as BossData;
        manager.animator.SetTrigger("Wound");
        runtime.getHit = false;
        timer = 0f;
    }

    public override void OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= stunDuration)
        {
            if (runtime.currentHealth <= 0)
            {
                manager.ChangeState(StateType.Dead);
                return;
            }

            if (bossData != null && ShouldSummonMinions())
            {
                SummonMinions();
            }

            if (runtime.target != null)
                manager.ChangeState(StateType.Chase);
            else
                manager.ChangeState(StateType.Patrol);
        }
    }

    private bool ShouldSummonMinions()
    {
        if (bossData.hasSummoned) return false;
        float healthPercent = runtime.currentHealth / bossData.health;
        return healthPercent <= bossData.summonHealthThreshold;
    }

    private void SummonMinions()
    {
        bossData.hasSummoned = true;

        if (controller.ownerRoom != null && controller.ownerRoom.waveManager != null)
        {
            controller.ownerRoom.waveManager.StartWave(controller.ownerRoom);
            Debug.Log($"[{controller.name}] 半血召唤！通知房间生成小怪");
        }
        else
        {
            Debug.LogWarning($"[{controller.name}] 无法召唤：ownerRoom 或 waveManager 为空");
        }
    }

    public override void OnExit() { }
}
