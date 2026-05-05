using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Enemy/Boss Data")]
public class BossData : MeleeEnemyData
{
    [Header("Boss 远程攻击")]
    public float rangedAttackRange = 12f;
    public float rangedAttackDamage = 15f;
    public GameObject rangedProjectilePrefab;
    public float rangedAttackCooldown = 3f;

    [Header("Boss 近战攻击")]
    public float meleeAttackRange = 3f;
    public float meleeAttackDamage = 25f;

    [Header("半血召唤")]
    public float summonHealthThreshold = 0.5f;
    public bool hasSummoned = false;
}
