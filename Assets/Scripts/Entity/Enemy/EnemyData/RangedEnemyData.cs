using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 远程敌人数据类，继承自EnemyData，添加了远程敌人特有的属性，如攻击范围、攻击伤害等，方便在编辑器中进行调整和管理。
/// </summary>
[CreateAssetMenu(fileName = "RangedEnemyData", menuName = "Enemy/Ranged Data")]
public class RangedEnemyData : EnemyData
{
    public float attackRange = 8f;
    public float Atk = 10;
}