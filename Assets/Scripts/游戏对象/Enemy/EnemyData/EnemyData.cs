using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyData : ScriptableObject
{
    public float health = 100f;
    public float moveSpeed = 5f;
    public float chaseSpeed = 7f;
    public float idleTime = 2f;
    public LayerMask targetLayer;

    // 视觉与特效
    public SpriteRenderer spriteRenderer; // 可自动获取，也可配置
    public Animator animator;
    public GameObject damageTextPrefab;
    public GameObject deadEff;
    public Collider2D chaseArea;

    // 巡逻设置
    public Transform patrolCenter;
    public Transform[] patrolPoints;
    public float patrolRadius = 5f;

    //敌人所属的房间
    public Room ownerRoom;

}