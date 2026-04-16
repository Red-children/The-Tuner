using UnityEngine;

[System.Serializable]
public class NoiseScreamWarningState : EnemyStateBase
{
    private float timer;
    private NoiseMonsterData noiseData;  // 改为 NoiseMonsterData

    public NoiseScreamWarningState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        noiseData = data as NoiseMonsterData;  // 改为 NoiseMonsterData
        if (noiseData == null)
        {
            manager.ChangeState(StateType.Chase);
            return;
        }

        controller.ShowAttackWarning();     // 显示攻击预警UI
        runtime.isVulnerable = true;        // 进入嘶吼预警状态时敌人变得脆弱，玩家可以通过攻击打断嘶吼
        timer = 0f;                         
        Debug.Log("噪声进入嘶吼预警状态");
    }

    public override void OnUpdate()
    {
        // 如果受到攻击，立即切换到受伤状态
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }

        // 如果目标丢失，切换回巡逻状态
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }

        timer += Time.deltaTime;
        if (timer >= noiseData.noiseWarningDuration)  // 注意字段名
        {
            manager.ChangeState(StateType.NoiseScreamAttack);
        }
    }

    public override void OnExit()
    {
        runtime.isVulnerable = false;
    }
}