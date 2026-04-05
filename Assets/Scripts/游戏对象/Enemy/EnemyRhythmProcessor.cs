using UnityEngine;

/// <summary>
/// 敌人节奏处理器 - 处理敌人与节奏的交互
/// </summary>
public static class EnemyRhythmProcessor
{
    /// <summary>
    /// 获取敌人移动速度倍率
    /// </summary>
    /// <returns>速度倍率</returns>
    public static float GetMoveSpeedMultiplier()
    {
        // 获取当前节奏等级
        var rankResult = RhythmManager.Instance.GetRank();
        
        // 根据节奏等级调整移动速度
        switch (rankResult.rank)
        {
            case RhythmRank.Perfect:
                return 1.2f; // 完美节拍时敌人移动速度略有提升
            case RhythmRank.Great:
                return 1.1f;
            case RhythmRank.Good:
                return 1.0f;
            default:
                return 0.9f; // 失误时敌人移动速度降低
        }
    }
    
    /// <summary>
    /// 检查是否是攻击的最佳时机
    /// </summary>
    /// <returns>是否是攻击时机</returns>
    public static bool IsAttackTiming()
    {
        // 获取当前节奏等级
        var rankResult = RhythmManager.Instance.GetRank();
        
        // 只有在完美或优秀节拍时才是最佳攻击时机
        return rankResult.rank == RhythmRank.Perfect || rankResult.rank == RhythmRank.Great;
    }
    
    /// <summary>
    /// 获取敌人攻击伤害倍率
    /// </summary>
    /// <returns>伤害倍率</returns>
    public static float GetAttackDamageMultiplier()
    {
        // 获取当前节奏等级
        var rankResult = RhythmManager.Instance.GetRank();
        
        // 根据节奏等级调整伤害
        switch (rankResult.rank)
        {
            case RhythmRank.Perfect:
                return 1.5f; // 完美节拍时敌人伤害提升
            case RhythmRank.Great:
                return 1.2f;
            case RhythmRank.Good:
                return 1.0f;
            default:
                return 0.8f; // 失误时敌人伤害降低
        }
    }
}
