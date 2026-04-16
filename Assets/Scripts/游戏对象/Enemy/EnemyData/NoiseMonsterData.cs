using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NoiseMonsterData", menuName = "Enemy/Noise Monster Data")]
public class NoiseMonsterData : MeleeEnemyData
{
    [Header("精英：噪声")]
    public bool isNoiseMonster = false;          // 勾选表示该数据属于噪声
    public float noiseScreamCooldown = 5f;       // 嘶吼冷却
    public float noiseWarningDuration = 0.5f;    // 预警时间
    public float noiseScreamRange = 10f;          // 嘶吼范围
    public int noiseScreamDamage = 20;           // 嘶吼伤害

    public float stunDuration = 3f;          //嘶吼持续时间
    
    public float noiseStunDuration = 3f;         // 被打断后眩晕时间
    public GameObject noiseScreamEffectPrefab;   // 声波特效


}
