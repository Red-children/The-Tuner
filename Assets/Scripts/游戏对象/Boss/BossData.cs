using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Data/BossData")]
public class BossData : EnemyData
{
    [Header("Boss 特有属性")]
    public float specialAttackDamage = 20f;
    public float phaseChangeHealthThreshold = 0.5f;
    public float normalAttackRange = 10f;

    [Header("技能设置")]
    public float skillCooldown = 5f;
    public float pullDistance = 1.5f;
    public float teleportOffset = 3f;

    [Header("架势系统")]
    public float maxPosture = 100f;
    public float postureRegenRate = 10f;
    public float postureRegenDelay = 2f;
    public float staggerDuration = 3f;
    public float postureDamageMultiplier = 1f;

    [Header("污染区域技能")]
    public GameObject contaminatedZonePrefab;    // 如果不使用动态生成，可留空，但建议用动态生成方式则无需此字段
    public float zoneDamagePerSecond = 10f;      // 每秒对玩家伤害
    public float zoneSpawnInterval = 3f;         // 小怪生成间隔（秒）
    public GameObject minionPrefab;              // 生成的小怪预制体
    public float zoneMaxHealth = 50f;            // 区域生命值
    public int maxZoneCount = 3;                 // 同时存在的最大区域数量
}