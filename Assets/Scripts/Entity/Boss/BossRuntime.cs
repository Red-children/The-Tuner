using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Boss运行时状态类，负责管理Boss在游戏运行时的动态状态，如当前目标、是否受击、当前血量、移动速度和追逐速度等，同时提供对原始SO数据的访问，确保在Boss的行为逻辑中能够根据当前状态做出相应的反应，增强了Boss的智能和互动性，同时通过运行时状态的设计模式实现了Boss状态的集中管理和扩展性，方便后续添加新的状态和行为逻辑。
/// </summary>
public class BossRuntime : MonoBehaviour
{
    [Header("运行时状态")]
    public Transform target;          // 当前目标
    public bool getHit;               // 是否受击
    public float currentHealth;       // 当前血量（运行时状态）
    public float currentMoveSpeed;   // 当前移动速度（运行时状态）
    public float currentChaseSpeed;  // 当前追逐速度（运行时状态）

    [Header("数据访问")]
    [SerializeField] private BossData originalData; // 原始SO引用
    
    // 只读数据访问器 - 保持类型兼容性
    public BossData Data => originalData;

    public void Init(BossData bossData)
    {
        // 保存原始SO引用
        originalData = bossData;
        
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



