using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近战敌人数据类，继承自EnemyData，添加了近战敌人特有的属性，如攻击范围、攻击伤害等，方便在编辑器中进行调整和管理。
/// </summary>
[CreateAssetMenu(fileName = "MeleeEnemyData", menuName = "Enemy/Melee Data")]
public class MeleeEnemyData : EnemyData
{
    public float attackAngle = 60f;
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public Vector2 attackOffset = new Vector2(1f, 0f);

    public float Atk = 10;

    
    [Header("追击配置")]
    [Tooltip("最大追击距离，超过此距离则放弃追击返回巡逻")]
    public float maxChaseDistance = 15f;

}
