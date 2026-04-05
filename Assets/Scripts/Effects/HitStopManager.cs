using UnityEngine;

/// <summary>
/// 卡肉感管理器：统一管理游戏中的卡肉感效果
/// 设计理念：与现有攻击系统无缝集成，提供灵活的配置选项
/// </summary>
public class HitStopManager : MonoBehaviour
{
    [Header("全局设置")]
    [SerializeField] private bool enableHitStop = true;             // 全局开关，允许玩家选择是否启用卡肉感
    [SerializeField] private float globalHitStopMultiplier = 1.0f;  // 全局强度乘数，允许调整所有卡肉感效果的强度（0.5 = 50%强度, 2.0 = 200%强度）

    [Header("默认配置")]
    [SerializeField] private float defaultHitStopDuration = 0.08f;  // 默认卡肉感持续时间（秒），适用于未指定等级的情况
    [SerializeField] private float perfectHitStopDuration = 0.12f;  // 完美判定的卡肉感持续时间（秒）
    [SerializeField] private float greatHitStopDuration = 0.08f;    // 极佳判定的卡肉感持续时间（秒）    
    [SerializeField] private float goodHitStopDuration = 0.04f;     // 良好判定的卡肉感持续时间（秒）

    private static HitStopManager _instance;        // 单例实例，方便全局访问
    public static HitStopManager Instance => _instance; // 全局访问点

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("卡肉感管理器已初始化");
    }
    
    /// <summary>
    /// 在敌人被命中时触发卡肉感
    /// </summary>
    /// <param name="enemy">被命中的敌人</param>
    /// <param name="rank">节奏判定等级</param>
    /// <param name="damage">伤害值（用于计算强度）</param>
    public void TriggerEnemyHitStop(GameObject enemy, RhythmRank rank, float damage = 0f)
    {
        if (!enableHitStop || enemy == null) return;
        
        // 获取敌人的LocalHitStop组件
        LocalHitStop hitStop = enemy.GetComponent<LocalHitStop>();
        if (hitStop == null)
        {
            // 如果没有组件，自动添加一个
            hitStop = enemy.AddComponent<LocalHitStop>();
            Debug.Log($"为敌人 {enemy.name} 添加了LocalHitStop组件");
        }
        
        // 根据判定等级和伤害计算强度
        float intensity = CalculateHitStopIntensity(rank, damage);                  
        float duration = GetHitStopDurationByRank(rank) * globalHitStopMultiplier;  

        // 触发卡肉感
        hitStop.TriggerHitStop(duration, intensity);
        
        Debug.Log($"敌人 {enemy.name} 触发卡肉感: {rank}, 强度: {intensity:F2}");
    }
    
    /// <summary>
    /// 在玩家被命中时触发卡肉感
    /// </summary>
    public void TriggerPlayerHitStop(RhythmRank rank, float damage = 0f)
    {
        if (!enableHitStop) return;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        LocalHitStop hitStop = player.GetComponent<LocalHitStop>();
        if (hitStop == null)
        {
            hitStop = player.AddComponent<LocalHitStop>();
            Debug.Log("为玩家添加了LocalHitStop组件");
        }
        
        float intensity = CalculateHitStopIntensity(rank, damage);                      //计算强度
        float duration = GetHitStopDurationByRank(rank) * globalHitStopMultiplier;      //计算持续时间

        hitStop.TriggerHitStop(duration, intensity);    // 触发卡肉感

        Debug.Log($"玩家触发卡肉感: {rank}, 强度: {intensity:F2}");
    }
    
    /// <summary>
    /// 在节奏判定成功时触发卡肉感（用于节奏游戏模式）
    /// </summary>
    public void TriggerRhythmHitStop(RhythmRank rank, GameObject target = null)
    {
        if (!enableHitStop) return;
        
        if (target != null)
        {
            // 对特定目标触发
            LocalHitStop hitStop = target.GetComponent<LocalHitStop>();
            if (hitStop == null)
            {
                hitStop = target.AddComponent<LocalHitStop>();
            }
            
            hitStop.TriggerHitStopByRank(rank);
        }
        else
        {
            // 全局轻微卡肉感（不影响游戏性）
            float duration = GetHitStopDurationByRank(rank) * 0.3f; // 全局效果更轻微
            StartCoroutine(GlobalHitStopCoroutine(duration));
        }
    }

    /// <summary>
    /// 全局轻微卡肉感协程，避免影响游戏性
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private System.Collections.IEnumerator GlobalHitStopCoroutine(float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.5f; // 轻微全局减速
        
        yield return new WaitForSecondsRealtime(duration);
        
        Time.timeScale = originalTimeScale;
    }
    
    private float CalculateHitStopIntensity(RhythmRank rank, float damage)
    {
        float rankIntensity = rank switch
        {
            RhythmRank.Perfect => 1.0f,
            RhythmRank.Great => 0.7f,
            RhythmRank.Good => 0.4f,
            _ => 0.2f
        };
        
        // 根据伤害值调整强度（可选）
        float damageIntensity = Mathf.Clamp01(damage / 100f); // 假设100为基准伤害
        
        return Mathf.Max(rankIntensity, damageIntensity);
    }
    
    private float GetHitStopDurationByRank(RhythmRank rank)
    {
        return rank switch
        {
            RhythmRank.Perfect => perfectHitStopDuration,
            RhythmRank.Great => greatHitStopDuration,
            RhythmRank.Good => goodHitStopDuration,
            _ => defaultHitStopDuration
        };
    }
    
    /// <summary>
    /// 启用/禁用卡肉感系统
    /// </summary>
    public void SetHitStopEnabled(bool enabled)
    {
        enableHitStop = enabled;
        Debug.Log($"卡肉感系统: {(enabled ? "已启用" : "已禁用")}");
    }
    
    /// <summary>
    /// 设置全局卡肉感强度乘数
    /// </summary>
    public void SetGlobalMultiplier(float multiplier)
    {
        globalHitStopMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
        Debug.Log($"全局卡肉感强度乘数设置为: {globalHitStopMultiplier:F2}");
    }
}