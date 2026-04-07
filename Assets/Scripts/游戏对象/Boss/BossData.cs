using System.Collections;

using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Data/BossData")]
public class BossData : EnemyData
{
    // Boss特有的属性，可以根据需要添加更多属性，如特殊攻击、阶段切换等
    public float specialAttackDamage = 20f;             // 特殊攻击伤害
    public float phaseChangeHealthThreshold = 0.5f;      // 阶段切换的血量百分比
    
}