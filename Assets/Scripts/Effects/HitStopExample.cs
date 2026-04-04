using UnityEngine;

/// <summary>
/// 卡肉感使用示例：展示如何在游戏中应用局部卡肉感效果
/// 这个脚本可以作为参考，帮助你理解如何集成卡肉感系统
/// </summary>
public class HitStopExample : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private KeyCode testKey = KeyCode.H;
    [SerializeField] private GameObject testTarget; // 测试目标（敌人或玩家）
    
    [Header("卡肉感参数")]
    [SerializeField] private RhythmRank testRank = RhythmRank.Perfect;
    [SerializeField] private float testDamage = 50f;
    
    private void Update()
    {
        // 测试按键：触发卡肉感效果
        if (Input.GetKeyDown(testKey))
        {
            TestHitStop();
        }
    }
    
    /// <summary>
    /// 测试卡肉感效果
    /// </summary>
    private void TestHitStop()
    {
        if (testTarget == null)
        {
            Debug.LogWarning("请设置测试目标");
            return;
        }
        
        if (HitStopManager.Instance == null)
        {
            Debug.LogWarning("卡肉感管理器未找到");
            return;
        }
        
        // 根据目标类型触发不同的卡肉感
        if (testTarget.CompareTag("Enemy"))
        {
            HitStopManager.Instance.TriggerEnemyHitStop(testTarget, testRank, testDamage);
            Debug.Log($"触发敌人卡肉感: {testRank}, 伤害: {testDamage}");
        }
        else if (testTarget.CompareTag("Player"))
        {
            HitStopManager.Instance.TriggerPlayerHitStop(testRank, testDamage);
            Debug.Log($"触发玩家卡肉感: {testRank}, 伤害: {testDamage}");
        }
        else
        {
            // 通用目标
            LocalHitStop hitStop = testTarget.GetComponent<LocalHitStop>();
            if (hitStop == null)
            {
                hitStop = testTarget.AddComponent<LocalHitStop>();
            }
            
            hitStop.TriggerHitStopByRank(testRank);
            Debug.Log($"触发通用目标卡肉感: {testRank}");
        }
    }
    
    /// <summary>
    /// 在Inspector中调用的测试方法
    /// </summary>
    [ContextMenu("测试完美判定卡肉感")]
    public void TestPerfectHitStop()
    {
        testRank = RhythmRank.Perfect;
        TestHitStop();
    }
    
    [ContextMenu("测试优秀判定卡肉感")]
    public void TestGreatHitStop()
    {
        testRank = RhythmRank.Great;
        TestHitStop();
    }
    
    [ContextMenu("测试良好判定卡肉感")]
    public void TestGoodHitStop()
    {
        testRank = RhythmRank.Good;
        TestHitStop();
    }
    
    [ContextMenu("禁用卡肉感系统")]
    public void DisableHitStopSystem()
    {
        if (HitStopManager.Instance != null)
        {
            HitStopManager.Instance.SetHitStopEnabled(false);
        }
    }
    
    [ContextMenu("启用卡肉感系统")]
    public void EnableHitStopSystem()
    {
        if (HitStopManager.Instance != null)
        {
            HitStopManager.Instance.SetHitStopEnabled(true);
        }
    }
}