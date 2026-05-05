using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Data/BossData")]
public class BossData : MeleeEnemyData
{
    [Header("Boss特有属性")]
    public float specialAttackDamage = 20f;
    public float phaseChangeHealthThreshold = 0.5f;
    public float normalAttackRange = 10f;

    [Header("架势系统")]
    public float maxPosture = 100f;
    public float postureRegenRate = 10f;
    public float postureRegenDelay = 2f;
    public float staggerDuration = 3f;
    public float postureDamageMultiplier = 1f;

    [Header("技能配置")]
    public float skillCooldown = 8f;
    public float skillMinRange = 3f;
    public float skillMaxRange = 8f;
}
