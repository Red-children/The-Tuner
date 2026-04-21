using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExplosiveEnemyData", menuName = "Enemy/Explosive Data")]
public class ExplosiveEnemyData : EnemyData
{
    public float explosionRadius = 3f;
    public int explosionDamage = 50;
    public GameObject explosionEffectPrefab;
}

