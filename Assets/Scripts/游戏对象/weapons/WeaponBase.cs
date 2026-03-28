using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Pistol,
    Shotgun,
    Rifle
}

public enum WeaponAttackType
{
    Single,  
    Multi,    
}

public struct PlayerFiredEvent { }


#region 数据体
[System.Serializable]
public class WeaponStats
{
    public int id;                       // 新增：武器唯一ID（可在 Inspector 中设置）
    public WeaponType weaponType;
    public string weaponName;
    public float damage = 10f;     //造成伤害
    public float fireRate = 0.5f;        // 攻击间隔（秒）
    public int maxAmmo = 10;
    public WeaponAttackType attackType = WeaponAttackType.Single;
    public int multiBulletCount = 5;     // 霰弹发射数量
    public GameObject bulletPrefab;      // 子弹Prefab
    public float reloadTime;  // 装填时间（秒）

    public float shakeIntensity = 0.01f;

}
#endregion

[CreateAssetMenu(fileName = "WeaponBase", menuName = "Weapon/WeaponBase")]

//数据库文件
public class WeaponBase : ScriptableObject
{
    public List<WeaponStats> weaponList;

    // 获取指定类型的武器数据
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