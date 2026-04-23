using System.Collections;

using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Data/BossData")]
public class BossData : EnemyData
{
    // Boss特有的属性，可以根据需要添加更多属性，如特殊攻击、阶段切换等
    public float specialAttackDamage = 20f;             // 特殊攻击伤害
    public float phaseChangeHealthThreshold = 0.5f;      // 阶段切换的血量百分比

    public float normalAttackRange = 10f;              // 普通攻击范围

    [Header("架势系统")]
    public float maxPosture = 100f;                // 架势值上限
    public float postureRegenRate = 10f;           // 每秒架势恢复量
    public float postureRegenDelay = 2f;           // 受伤后多少秒开始恢复架势
    public float staggerDuration = 3f;             // 失衡持续时间
    public float postureDamageMultiplier = 1f;     // 架势伤害倍率（用于技能调整）

}