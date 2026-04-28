using System.Collections;
using UnityEngine;

public enum WeaponOwner
{
    Player,
    Enemy
}

public struct AmmoChangedEvent
{
    public int currentAmmo;     //当前子弹     
    public int reserveAmmo;     //预备子弹（如果有的话，暂时用不到）
    public int weaponId;        //武器ID，方便UI等模块识别是哪把武器的子弹发生了变化
}

public class WeaponInfo : MonoBehaviour
{
    [Header("武器配置")]
    public WeaponBase weaponBase;   // 静态的SO配置数据，包含武器的属性
    public WeaponType weaponType;   // 武器的类型，用于从weaponBase中获取对应的WeaponStats
    public Transform firePoint;     // 发射点，用于生成子弹的位置和方向

    private WeaponStats weaponStats; // 当前武器的属性数据，从weaponBase获取
    private int currentAmmo;
    private double lastShootTime;
    private bool isReloading;

    public WeaponOwner owner;   // Player �� Enemy

    #region  低音炮武器所需参数
    [Header("低音炮武器所需参数")]
    private bool isCharging = false;
    private float chargeTimer = 0f;

    #endregion

    // 对外提供只读属性，方便其他系统获取当前子弹数量和武器ID
    public int CurrentAmmo => currentAmmo;
    public int WeaponId => weaponStats?.id ?? -1;

    private void Awake()
    {
        if (weaponBase == null)
        {
            return;
        }
        weaponStats = weaponBase.GetWeaponStats(weaponType);
        if (weaponStats == null)
        {
            return;
        }
        currentAmmo = weaponStats.maxAmmo;
    }

    public void Shoot(float damage, float rhythmMultiplier, RhythmRank rank)
    {
        if (isReloading) return;
        if (weaponStats == null)
        {
            Debug.LogError($"[WeaponInfo] {gameObject.name} 武器数据为null，无法射击");
            return;
        }
        double currentTime = AudioSettings.dspTime;
        if (currentTime < lastShootTime + weaponStats.fireRate) return;
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

        float finalDamage = (damage + weaponStats.damage) * rhythmMultiplier;
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = weaponStats.shakeIntensity });
        SpawnBullet(finalDamage, rank);

        currentAmmo--;
        lastShootTime = currentTime;
        OnShootEffects();
    }

    private void SpawnBullet(float damage, RhythmRank rank)
    {
        GameObject bulletObj = Instantiate(weaponStats.bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.currentRhythmRank = rank;
        bullet.SetDamage(damage);

        // 缓存玩家位置（子弹发射时的位置）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            bullet.SetAttackerPosition(player.transform.position);
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
        // ���������¼�
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = weaponStats.shakeIntensity });
        // ���ڴ˴�����ǹ����Ч����Ч��
    }

    // ��ѡ�����ⲿ��ѯ��ǰ��ҩ����UIˢ�£�
    public int GetCurrentAmmo() => currentAmmo;

    #region   低音炮武器测试相关
    // 蓄力相关字段

    /// <summary>
    /// 生成蓄力子弹（穿透AOE或普通子弹）
    /// </summary>
    private void SpawnChargeProjectile(float damage, bool isPerfect)
    {
        // 复用现有的子弹生成逻辑
        GameObject bulletObj = Instantiate(weaponStats.bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(damage);

            // Perfect 释放时开启穿透
            if (isPerfect)
            {
                bullet.EnablePenetration(5, 0.1f);
                // 可选：子弹颜色变化
                SpriteRenderer sr = bulletObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 0.6f, 0f);
            }
        }


    }

    /// <summary>
    /// 开始蓄力（由 PlayerAttack 调用）
    /// </summary>
    public void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;
    }

    /// <summary>
    /// 释放蓄力（由 PlayerAttack 调用）
    /// </summary>
    public void ReleaseCharge()
    {
        if (!isCharging) return;

        float progress = Mathf.Clamp01(chargeTimer / weaponStats.chargeTime);

        if (progress >= 1.0f)
        {
            // 炸膛惩罚
            SelfDamage(weaponStats.perfectChargeDamage * weaponStats.selfDamageRatio);
            EventBus.Instance.Trigger(new CameraShakeEvent { intensity = 0.2f });
        }
        else if (progress >= weaponStats.overchargeThreshold)
        {
            // 判定节奏等级
            var rank = RhythmManager.Instance.GetRank();
            float finalDamage = rank.rank == RhythmRank.Perfect
                              ? weaponStats.perfectChargeDamage
                              : weaponStats.missChargeDamage;

            SpawnChargeProjectile(finalDamage, rank.rank == RhythmRank.Perfect);
            EventBus.Instance.Trigger(new CameraShakeEvent { intensity = rank.rank == RhythmRank.Perfect ? 0.4f : 0.15f });
        }
        else
        {
            // 未蓄满释放
            SpawnChargeProjectile(weaponStats.damage * weaponStats.weakReleaseDamageRatio, false);
        }

        isCharging = false;
        chargeTimer = 0f;
    }
    private void SelfDamage(float damage)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerAPI playerAPI = playerObj.GetComponent<PlayerAPI>();
            if (playerAPI != null)
                playerAPI.TakeDamage(Mathf.RoundToInt(damage));
        }
    }

    #endregion

    void Update()
    {


        // 蓄力震动反馈
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(chargeTimer / weaponStats.chargeTime);
            float shakeIntensity = Mathf.Lerp(weaponStats.chargeShakeBase, weaponStats.chargeShakeMax, progress);
            EventBus.Instance.Trigger(new CameraShakeEvent { intensity = shakeIntensity });
        }
    }
}