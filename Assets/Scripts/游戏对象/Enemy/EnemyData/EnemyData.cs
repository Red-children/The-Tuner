using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人数据类，使用ScriptableObject来存储敌人的属性和配置，方便在编辑器中进行调整和管理。
/// </summary>
public abstract class EnemyData : ScriptableObject
{
        // 敌人的基本属性，可以根据需要添加更多属性，如攻击力、防御力等
        public float health = 100f;
        public float moveSpeed = 5f;
        public float chaseSpeed = 7f;
        public float idleTime = 2f;
        public LayerMask targetLayer;
        public GameObject damageTextPrefab;
        public GameObject deadEff;
        public float patrolRadius = 5f;

}