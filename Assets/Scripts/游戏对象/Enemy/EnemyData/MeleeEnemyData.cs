using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeEnemyData", menuName = "Enemy/Melee Data")]
public class MeleeEnemyData : EnemyData
{
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public Vector2 attackOffset = new Vector2(1f, 0f);
    public Transform attackPoint;
}
