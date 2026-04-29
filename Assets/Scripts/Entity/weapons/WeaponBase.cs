using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Pistol,
    Shotgun,
    Rifle,
    
    BassCannon
}
/// <summary>
/// 武器攻击类型
/// </summary>
public enum WeaponAttackType
{
    Single,
    Multi,
}

public struct PlayerFiredEvent { }


#region 武器统计数据类
[System.Serializable]
public class WeaponStats
{
    public int id;                        // 武器唯一ID
    public WeaponType weaponType;        // 武器类型
    public string weaponName;
    public float damage = 10f;     // 伤害值
    public float fireRate = 0.5f;        // 射击频率
    public int maxAmmo = 10;
    public WeaponAttackType attackType = WeaponAttackType.Single;
    public int multiBulletCount = 5;     // 多子弹数量
    public GameObject bulletPrefab;      // 子弹Prefab
    public float reloadTime;  // 重新时间间隔

    public float shakeIntensity = 0.01f;


    [Header("蓄力配置（仅 BassCannon 等蓄力武器有效）")]
    public float chargeTime = 1.2f;              // 蓄满所需时间
    public float overchargeThreshold = 0.85f;    // 超过此比例允许释放
    public float perfectChargeDamage = 60f;      // Perfect 释放伤害
    public float missChargeDamage = 15f;         // Miss 释放伤害
    public float selfDamageRatio = 0.3f;         // 炸膛自伤比例
    public float weakReleaseDamageRatio = 0.3f;  // 未蓄满时释放的基础伤害比例
    public float chargeShakeBase = 0.03f;        // 蓄力初始震幅
    public float chargeShakeMax = 0.15f;         // 蓄力最大震幅

}
#endregion

[CreateAssetMenu(fileName = "WeaponBase", menuName = "Weapon/WeaponBase")]

// 武器基础数据类
public class WeaponBase : ScriptableObject
{
    public List<WeaponStats> weaponList;

    /// <summary>
    /// 获取武器统计数据
    /// </summary>
    /// <param name="type">武器类型</param>
    /// <returns>武器统计数据</returns>
    public WeaponStats GetWeaponStats(WeaponType type)
    {
        foreach (var weapon in weaponList)
        {
            if (weapon.weaponType == type)
                return weapon;
        }
        return null;
    }
}