using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 噪声眩晕状态类，负责处理敌人在被完美打断后进入眩晕状态的行为逻辑，包括计时器的更新以及状态切换，当眩晕时间结束后自动切换回追逐状态。
/// </summary>
[System.Serializable]
public class NoiseStunState : EnemyStateBase
{
    private float timer;
    private NoiseMonsterData noiseData;  // 改为 NoiseMonsterData

    public NoiseStunState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        noiseData = data as NoiseMonsterData;  // 改为 NoiseMonsterData
        timer = 0f;
        manager.animator.SetTrigger("Stun");  //播放眩晕动画
    }

    public override void OnUpdate()
    {
        timer += Time.deltaTime;              //计算眩晕时长
        if (timer >= noiseData.stunDuration)  // 注意字段名
        {
            manager.ChangeState(StateType.Chase);
        }
    }
}