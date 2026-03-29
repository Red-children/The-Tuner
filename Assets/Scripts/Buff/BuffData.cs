using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType 
{
    IncreaseDamage,     // 增加攻击力
    IncreaseFireRate,   // 增加射速
    IncreaseMoveSpeed,  // 增加移动速度
    IncreaseMaxHealth,  // 增加最大生命值
    IncreaseArmor,      // 增加防御力
    IncreaseCritRate,   // 增加暴击率
    IncreaseCritDamage, // 增加暴击伤害
    LifeSteal,          // 吸血（造成伤害时回复生命）
    BulletBounce,       // 子弹反弹
    BulletSplit,        // 子弹分裂
    DashCooldownReduce, // 闪避冷却减少
    InvincibleAfterHit, // 受伤后短暂无敌
    // 你可以根据需要继续添加
}

[Serializable]
public class BuffData
{
    public string buffName;
    public string description;
    public Sprite icon;
    public float value;               // 数值型Buff的增量（如攻击力+10）
    public BuffType type;              // 枚举：增伤、攻速、移速、特殊效果等
    public bool isStackable;           // 是否可叠加
    public int maxStack;                // 最大叠加层数
    


    // 可选：对于机制改变型Buff，可以挂载一个MonoBehaviour脚本
    public GameObject effectPrefab;    // 用于生成特效或挂载逻辑脚本
}

[CreateAssetMenu(fileName = "NewBuff", menuName = "Buff/BuffData")]
public class BuffDataList :ScriptableObject
{
    public List<BuffData> buffDatas;
}