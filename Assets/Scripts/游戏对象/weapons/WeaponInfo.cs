using System.Collections;
using UnityEngine;

public enum WeaponOwner
{
    Player,
    Enemy
}

public struct AmmoChangedEvent
{
    public int currentAmmo;
    public int reserveAmmo;   // ���б���ϵͳ��������-1
    public int weaponId;      // �� WeaponStats.id ��ȡ
}

public class WeaponInfo : MonoBehaviour
{
    [Header("��������")]
    public WeaponBase weaponBase;   // ScriptableObject ����Դ
    public WeaponType weaponType;   // ��ǰʹ�õ���������
    public Transform firePoint;      // �ӵ������

    private WeaponStats weaponStats; // �� weaponBase ��ȡ�ľ�̬����
    private int currentAmmo;
    private double lastShootTime;
    private bool isReloading;

    public WeaponOwner owner;   // Player �� Enemy


    // ���Ⱪ¶��ֻ������
    public int CurrentAmmo => currentAmmo;
    public int WeaponId => weaponStats?.id ?? -1;

    private void Awake()
    {
        if (weaponBase == null)
        {
            Debug.LogError($"[WeaponInfo] {gameObject.name} ȱ�� weaponBase ���ã�");
            return;
        }
        weaponStats = weaponBase.GetWeaponStats(weaponType);
        if (weaponStats == null)
        {
            Debug.LogError($"[WeaponInfo] δ�ҵ��������� {weaponType} ������");
            return;
        }
        currentAmmo = weaponStats.maxAmmo;
    }

    /// <summary>
    /// ����ӿڣ��ɵ��÷��ṩ�˺��ͽ��౶�ʡ�
    /// </summary>
    /// <param name="damage">���ι����Ļ����˺����Ѱ�����ɫ������ + ���������˺���</param>
    /// <param name="rhythmMultiplier">���౶�ʣ�ͨ������ RhythmManager��</param>
    public void Shoot(float damage, float rhythmMultiplier)
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

        float finalDamage = (damage+weaponStats.damage)  * rhythmMultiplier;
        SpawnBullet(finalDamage);

        currentAmmo--;
        lastShootTime = currentTime;
        OnShootEffects();
    }

    private void SpawnBullet(float damage)
    {
        if (weaponStats == null || weaponStats.bulletPrefab == null)
        {
            Debug.LogError($"[WeaponInfo] {gameObject.name} 子弹预制体未设置");
            return;
        }
        if (firePoint == null)
        {
            Debug.LogError($"[WeaponInfo] {gameObject.name} 发射点未设置");
            return;
        }
        GameObject bulletObj = Instantiate(weaponStats.bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(damage);
        }
        else
        {
            Debug.LogError($"[WeaponInfo] {gameObject.name} 子弹预制体缺少Bullet组件");
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
        // ���������¼�
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = weaponStats.shakeIntensity });
        // ���ڴ˴�����ǹ����Ч����Ч��
    }

    // ��ѡ�����ⲿ��ѯ��ǰ��ҩ����UIˢ�£�
    public int GetCurrentAmmo() => currentAmmo;
}