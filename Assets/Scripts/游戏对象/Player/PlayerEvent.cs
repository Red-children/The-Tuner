using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 用来传递节奏数据的事件结构体 供玩家和武器监听
public struct RhythmHitEvent
{
    public RhythmRank rank;      // 判定等级
    public float intensity;      // 根据等级决定的强度（可选）


}
#endregion

public struct PlayerFireEvent
{
    public bool isPerfect;   // 是否完美命中（根据你的判定等级）
    public RhythmRank rank;   // 可选，传递具体等级
}



public struct PlayerDiedEvent
{
    public PlayerIObject player;
}


public struct PlayerAtkChange 
{
    public int atk;
    
}
public struct PlayerHurtEvent
{
    public float damage;        // 原始伤害值（可用于计算护盾消耗等）
    public bool isBlocked;      // 是否被护盾/无敌等完全抵挡（不扣血）
}