using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 远程敌人攻击类型
/// </summary>
public enum RangedAttackType
{
    SingleShot,   // 单发
    Barrage       // 连射/散射
}

/// <summary>
/// 远程敌人数据类，继承自EnemyData，添加了远程敌人特有的属性，如攻击范围、攻击伤害等，方便在编辑器中进行调整和管理。
/// </summary>
[CreateAssetMenu(fileName = "RangedEnemyData", menuName = "Enemy/Ranged Data")]
public class RangedEnemyData : EnemyData
{
    public float attackRange = 8f;
    public float Atk = 10;

    [Header("多攻击配置")]
    [Tooltip("每种攻击类型对应的特效预制体")]
    public GameObject[] attackEffectPrefabs;

    [Tooltip("每种攻击类型对应的伤害值")]
    public float[] attackDamages;

    [Tooltip("每种攻击类型对应的动画参数值（对应Animator中的AttackIndex）")]
    public int[] attackAnimationIndex;
}