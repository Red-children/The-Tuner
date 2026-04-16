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
}