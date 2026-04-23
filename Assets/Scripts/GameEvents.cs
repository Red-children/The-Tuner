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
    public int count;
    public RhythmRank rank;
    public EnemyHitEvent(int count, RhythmRank rank)
    {
        this.count = count;
        this.rank = rank;
    }
}
public struct PlayerAtkEvent
{
    //  TODO:
}

public struct RhythmInputDebugEvent
{
    public double inputDspTime;
    public double judgedDspTime;
    public double referenceBeatTime;
    public double offsetSeconds;
    public double offsetMilliseconds;
    public RhythmRank rank;
    public float multiplier;
    public bool isInWindow;
    public string source;
}

public struct AttackMultiplierChangedEvent
{
    public float newMultiplier; // 最新倍率
    public bool isCritical; // 是否暴击
    public double time; // 事件发生时间
}




public struct PlayBGMEvent
{
    //  TODO:
    public BGMData data;
    public PlayBGMEvent(BGMData d)
    {
        data = d;
    }
}
public struct BGMProgressUpdateEvent
{
    public double Progress;
    public double PreciseTime;
    public double TotalDuration;
}
public struct IndicatorActiveEvent
{
    // public double BPM;   //  1s/BPM
    public double advance;  //  预计提前时间
    public double time;     //  已播放时间
    public double nextPoint;    //  下一个关键点在音乐中的时间
}

