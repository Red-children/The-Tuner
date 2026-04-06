using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//敌人参数
public abstract class EnemyData : ScriptableObject
{
        // 纯数值
        public float health = 100f;
        public float moveSpeed = 5f;
        public float chaseSpeed = 7f;
        public float idleTime = 2f;
        public LayerMask targetLayer;
        public GameObject damageTextPrefab;
        public GameObject deadEff;
        public float patrolRadius = 5f;
        // 移除所有 Transform、SpriteRenderer、Animator 等场景引用
}