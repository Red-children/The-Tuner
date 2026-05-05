using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Enemy/Boss Data")]
public class BossData : RangedEnemyData
{
    [Header("Boss 远程攻击")]
    public GameObject rangedProjectilePrefab;
    public float projectileSpeed = 8f;

    [Header("半血召唤")]
    public float summonHealthThreshold = 0.5f;
    public bool hasSummoned = false;
}
