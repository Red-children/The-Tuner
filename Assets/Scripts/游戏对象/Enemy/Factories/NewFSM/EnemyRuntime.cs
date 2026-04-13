using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人运行时数据 - 安全的SO数据访问机制
/// 保持类型兼容性，避免直接修改ScriptableObject资产文件
/// </summary>
public class EnemyRuntime : MonoBehaviour
{
    [Header("运行时状态")]
    public Transform target;          // 当前目标
    public bool getHit;               // 是否受击
    public float currentHealth;       // 当前血量（运行时状态）
    public float currentMoveSpeed;   // 当前移动速度（运行时状态）
    public float currentChaseSpeed;  // 当前追逐速度（运行时状态）

    [Header("数据访问")]
    [SerializeField] private EnemyData originalData; // 原始SO引用
    
    // 只读数据访问器 - 保持类型兼容性
    public EnemyData Data => originalData;

    public void Init(EnemyData enemyData)
    {
        // 保存原始SO引用
        originalData = enemyData;
        
        // 初始化运行时状态（从SO读取初始值）
        currentHealth = originalData.health;
        currentMoveSpeed = originalData.moveSpeed;
        currentChaseSpeed = originalData.chaseSpeed;
    }

    /// <summary>
    /// 安全的属性访问方法
    /// 返回运行时状态或原始SO数据
    /// </summary>
    public float Health => currentHealth;
    public float MoveSpeed => currentMoveSpeed;
    public float ChaseSpeed => currentChaseSpeed;
    
    // 只读属性 - 直接返回SO数据（这些数据不会被修改）
    public float IdleTime => originalData.idleTime;
    public LayerMask TargetLayer => originalData.targetLayer;
    public GameObject DamageTextPrefab => originalData.damageTextPrefab;
    public GameObject DeadEff => originalData.deadEff;
    public float PatrolRadius => originalData.patrolRadius;
}

