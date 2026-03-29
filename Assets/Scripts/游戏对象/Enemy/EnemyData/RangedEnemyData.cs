using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RangedEnemyData", menuName = "Enemy/Ranged Data")]
public class RangedEnemyData : EnemyData
{
    public float attackRange = 8f;
    public float Atk = 10;
}