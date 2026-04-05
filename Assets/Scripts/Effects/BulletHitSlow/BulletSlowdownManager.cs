using UnityEngine;

/// <summary>
/// 子弹减速管理器：统一管理游戏中子弹的减速效果
/// 设计理念：与现有子弹系统无缝集成，提供灵活的配置选项
/// </summary>
public class BulletSlowdownManager : MonoBehaviour
{
    [Header("全局设置")]
    [SerializeField] private bool enableBulletSlowdown = true;
    [SerializeField] private float globalSlowdownMultiplier = 1.0f;
    
    [Header("默认配置")]
    [SerializeField] private float defaultSlowdownDuration = 0.1f;
    [SerializeField] private float perfectSlowdownDuration = 0.15f;
    [SerializeField] private float greatSlowdownDuration = 0.12f;
    [SerializeField] private float goodSlowdownDuration = 0.08f;
    
    [Header("减速强度")]
    [SerializeField] private float perfectSlowdownFactor = 0.2f;
    [SerializeField] private float greatSlowdownFactor = 0.3f;
    [SerializeField] private float goodSlowdownFactor = 0.4f;
    
    private static BulletSlowdownManager _instance;
    public static BulletSlowdownManager Instance => _instance;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("子弹减速管理器已初始化");
    }
    
    /// <summary>
    /// 为子弹添加减速组件并触发效果
    /// </summary>
    /// <param name="bullet">子弹对象</param>
    /// <param name="rank">节奏判定等级</param>
    /// <param name="damage">伤害值（用于计算强度）</param>
    public void TriggerBulletSlowdown(GameObject bullet, RhythmRank rank, float damage = 0f)
    {
        if (!enableBulletSlowdown || bullet == null) return;
        
        // 获取子弹的减速组件
        BulletHitSlowdown slowdown = bullet.GetComponent<BulletHitSlowdown>();
        if (slowdown == null)
        {
            // 如果没有组件，自动添加一个
            slowdown = bullet.AddComponent<BulletHitSlowdown>();
            Debug.Log($"为子弹 {bullet.name} 添加了BulletHitSlowdown组件");
        }
        
        // 根据判定等级和伤害计算强度
        float intensity = CalculateSlowdownIntensity(rank, damage);
        
        // 触发减速效果
        slowdown.TriggerSlowdownByRank(rank);
        
        Debug.Log($"子弹 {bullet.name} 触发减速: {rank}, 强度: {intensity:F2}");
    }
    
    /// <summary>
    /// 直接为子弹配置减速参数
    /// </summary>
    public void ConfigureBulletSlowdown(GameObject bullet, float duration, float factor)
    {
        if (bullet == null) return;
        
        BulletHitSlowdown slowdown = bullet.GetComponent<BulletHitSlowdown>();
        if (slowdown == null)
        {
            slowdown = bullet.AddComponent<BulletHitSlowdown>();
        }
        
        // 这里可以添加配置逻辑（如果需要的话）
        Debug.Log($"配置子弹减速参数: 持续时间{duration:F2}s, 减速系数{factor:F2}");
    }
    
    /// <summary>
    /// 根据子弹类型获取默认减速配置
    /// </summary>
    public (float duration, float factor) GetDefaultSlowdownConfig(string bulletType)
    {
        return bulletType.ToLower() switch
        {
            "pistol" => (0.08f, 0.4f),     // 手枪子弹：轻微减速
            "shotgun" => (0.12f, 0.2f),    // 霰弹枪：强力减速
            "sniper" => (0.15f, 0.1f),     // 狙击枪：极强减速
            "laser" => (0.05f, 0.6f),      // 激光：快速恢复
            _ => (defaultSlowdownDuration, 0.3f) // 默认配置
        };
    }
    
    private float CalculateSlowdownIntensity(RhythmRank rank, float damage)
    {
        float rankIntensity = rank switch
        {
            RhythmRank.Perfect => 1.0f,
            RhythmRank.Great => 0.8f,
            RhythmRank.Good => 0.6f,
            _ => 0.4f
        };
        
        // 根据伤害值调整强度（可选）
        float damageIntensity = Mathf.Clamp01(damage / 50f); // 假设50为基准伤害
        
        return Mathf.Max(rankIntensity, damageIntensity) * globalSlowdownMultiplier;
    }
    
    /// <summary>
    /// 根据判定等级获取减速持续时间
    /// </summary>
    public float GetSlowdownDurationByRank(RhythmRank rank)
    {
        return rank switch
        {
            RhythmRank.Perfect => perfectSlowdownDuration,
            RhythmRank.Great => greatSlowdownDuration,
            RhythmRank.Good => goodSlowdownDuration,
            _ => defaultSlowdownDuration
        };
    }
    
    /// <summary>
    /// 根据判定等级获取减速系数
    /// </summary>
    public float GetSlowdownFactorByRank(RhythmRank rank)
    {
        return rank switch
        {
            RhythmRank.Perfect => perfectSlowdownFactor,
            RhythmRank.Great => greatSlowdownFactor,
            RhythmRank.Good => goodSlowdownFactor,
            _ => 0.5f // 默认系数
        };
    }
    
    /// <summary>
    /// 启用/禁用子弹减速系统
    /// </summary>
    public void SetSlowdownEnabled(bool enabled)
    {
        enableBulletSlowdown = enabled;
        Debug.Log($"子弹减速系统: {(enabled ? "已启用" : "已禁用")}");
    }
    
    /// <summary>
    /// 设置全局减速强度乘数
    /// </summary>
    public void SetGlobalMultiplier(float multiplier)
    {
        globalSlowdownMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
        Debug.Log($"全局子弹减速强度乘数设置为: {globalSlowdownMultiplier:F2}");
    }
}