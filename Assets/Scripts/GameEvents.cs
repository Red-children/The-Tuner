using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TimerOnlineEvent
{
    //  TODO:
    public float time; // 事件发生时间
}

public struct EnemyHitEvent
    {
    //  TODO:
    }
public struct PlayerAtkEvent
{
    //  TODO:
}

public struct GlobalAttackMultiplierChangedEvent
{
    public float newMultiplier; // 最新倍率
    public bool isCritical; // 是否暴击
    public float time; // 事件发生时间
}

