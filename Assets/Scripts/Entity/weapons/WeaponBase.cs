using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Pistol,
    Shotgun,
    Rifle,
    
    BassCannon
}

public enum WeaponAttackType
{
    Single,
    Multi,
}

public struct PlayerFiredEvent { }


#region ������
[System.Serializable]
public class WeaponStats
{
    public int id;                       // ����������ΨһID������ Inspector �����ã�
    public WeaponType weaponType;
    public string weaponName;
    public float damage = 10f;     //����˺�
    public float fireRate = 0.5f;        // ����������룩
    public int maxAmmo = 10;
    public WeaponAttackType attackType = WeaponAttackType.Single;
    public int multiBulletCount = 5;     // ������������
    public GameObject bulletPrefab;      // �ӵ�Prefab
    public float reloadTime;  // װ��ʱ�䣨�룩

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

//���ݿ��ļ�
public class WeaponBase : ScriptableObject
{
    public List<WeaponStats> weaponList;

    // ��ȡָ�����͵���������
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