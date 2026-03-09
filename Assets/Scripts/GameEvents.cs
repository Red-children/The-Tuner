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

public struct AttackMultiplierChangedEvent
{
    public float newMultiplier; // 最新倍率
    public bool isCritical; // 是否暴击
    public double time; // 事件发生时间
}




struct PlayBGMEvent
{
    //  TODO:
    public double time; // 事件发生时间
}
struct BGMProgressUpdateEvent
{
    public double Progress;
    public double PreciseTime;
    public double TotalDuration;
}

struct IndicatorActiveEvent
{
    public double time;     //  已播放时间
    public double nextPoint;    //  下一个关键点在音乐中的时间
}