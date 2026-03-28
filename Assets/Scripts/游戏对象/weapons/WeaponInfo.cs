using System.Collections;
using System.Security.Cryptography;
using UnityEngine;



public enum WeaponOwner
{
    Player,
    Enemy
}

public struct AmmoChangedEvent
{
    public int currentAmmo;
    public int reserveAmmo;   // 若有备弹系统，否则填-1
    public int weaponId;      // 从 WeaponStats.id 获取
}

public class WeaponInfo : MonoBehaviour
{
    [Header("武器配置")]
    public WeaponBase weaponBase;   // ScriptableObject 数据源
    public WeaponType weaponType;   // 当前使用的武器类型
    public Transform firePoint;      // 子弹发射点

    private WeaponStats weaponStats; // 从 weaponBase 获取的静态数据
    private int currentAmmo;
    private float lastShootTime;
    private bool isReloading;

    public WeaponOwner owner;   // Player 或 Enemy


    // 对外暴露的只读属性
    public int CurrentAmmo => currentAmmo;
    public int WeaponId => weaponStats?.id ?? -1;

    private void Awake()
    {
        if (weaponBase == null)
        {
            Debug.LogError($"[WeaponInfo] {gameObject.name} 缺少 weaponBase 引用！");
            return;
        }
        weaponStats = weaponBase.GetWeaponStats(weaponType);
        if (weaponStats == null)
        {
            Debug.LogError($"[WeaponInfo] 未找到武器类型 {weaponType} 的数据");
            return;
        }
        currentAmmo = weaponStats.maxAmmo;
    }

    /// <summary>
    /// 射击接口，由调用方提供伤害和节奏倍率。
    /// </summary>
    /// <param name="damage">本次攻击的基础伤害（已包含角色攻击力 + 武器基础伤害）</param>
    /// <param name="rhythmMultiplier">节奏倍率（通常来自 RhythmManager）</param>
    public void Shoot(float damage, float rhythmMultiplier)
    {
        if (isReloading) return;
        if (Time.time < lastShootTime + weaponStats.fireRate) return;
        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }
        if (owner == WeaponOwner.Player)
        {
            EventBus.Instance.Trigger(new AmmoChangedEvent
            {
                currentAmmo = currentAmmo,
                reserveAmmo = -1,
                weaponId = weaponStats.id
            });
        }

        float finalDamage = (damage + weaponStats.damage + bonusDamage) * rhythmMultiplier;
        if (weaponStats.attackType == WeaponAttackType.Single)
        {
            SpawnBullet(finalDamage, firePoint.rotation);
        }
        else if (weaponStats.attackType == WeaponAttackType.Multi)
        {
            ShootMulti(finalDamage);
        }

        currentAmmo--;
        lastShootTime = Time.time;
        OnShootEffects();
    }

    private void SpawnBullet(float damage, Quaternion rotation)
    {
        GameObject bulletObj = Instantiate(weaponStats.bulletPrefab, firePoint.position, rotation);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(damage);
        }
    }

    private void StartReload()
    {
        if (isReloading) return;
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(weaponStats.reloadTime);
        currentAmmo = weaponStats.maxAmmo;
        isReloading = false;

        EventBus.Instance.Trigger(new AmmoChangedEvent
        {
            currentAmmo = currentAmmo,
            reserveAmmo = -1,
            weaponId = weaponStats.id
        });
    }

    private void OnShootEffects()
    {
        // 触发震屏事件
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = weaponStats.shakeIntensity });
        // 可在此处播放枪口特效、音效等
    }

    // 可选：供外部查询当前弹药（如UI刷新）
    public int GetCurrentAmmo() => currentAmmo;

    private void ShootMulti(float damage)
    {
        int count = weaponStats.multiBulletCount;

        if (count <= 0)
        {
            Debug.LogWarning("Shotgun 子弹数量 <= 0！");
            return;
        }

        float spreadAngle = 30f;

        if (count == 1)
        {
            SpawnBullet(damage, firePoint.rotation);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.Lerp(-spreadAngle / 2, spreadAngle / 2, (float)i / (count - 1));

            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            SpawnBullet(damage, rotation);
        }
    }

    private float bonusDamage = 0f;

    public void AddDamage(float amount)
    {
        bonusDamage += amount;
    }
}