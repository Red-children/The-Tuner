using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cd敌人数据类，继承自EnemyData，添加了Cd敌人特有的属性
/// </summary>
[CreateAssetMenu(fileName = "CdData", menuName = "Enemy/Cd Data")]
public class CdData : EnemyData
{
    [Header("攻击配置")]
    [Tooltip("攻击角度，用于计算攻击范围")]
    public float attackAngle = 60f;
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public Vector2 attackOffset = new Vector2(1f, 0f);

    [Header("敌人进入接近状态开始调整攻击的距离")]
    public float approachDistance;

    [Header("敌人与玩家间保持的最小距离如果低于这个距离敌人将不会移动")]
    public float stopMinRange;

    public float Atk = 10;

    [Header("追击配置")]
    [Tooltip("最大追击距离，超过此距离则放弃追击返回巡逻")]
    public float maxChaseDistance = 15f;

    [Header("攻击特效")]
    [Tooltip("敌人攻击时播放的特效预制体")]
    public GameObject attackEffectPrefab;

    [Header("Cd敌人特有配置")]
    [Tooltip("Cd敌人特有属性")]
    public float cdSpecialAttribute = 1f;
}