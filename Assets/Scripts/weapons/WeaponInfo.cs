using System.Collections;
using UnityEngine;

public class WeaponInfo : MonoBehaviour
{
    [Header("Weapon Display Info")]
    public string weaponName;
    public string obtainMethod;
    public Sprite weaponSprite;

    [Header("Weapon Data")]
    public WeaponBase weaponBase;       // 数据库
    public WeaponType weaponType;       // 当前武器
    public Transform firePos;           // 射击位置

    private WeaponStats stats;
    private int currentAmmo;
    private float lastFireTime;
    private bool isReloading;

    void Start()
    {
        stats = weaponBase.GetWeaponStats(weaponType);
        weaponName = stats.weaponName;
        currentAmmo = stats.maxAmmo;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
            Shoot();

        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Reload());

        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (isReloading || currentAmmo <= 0)
            return;

        if (Time.time - lastFireTime < stats.fireRate)
            return;

        lastFireTime = Time.time;

        if (stats.attackType == WeaponAttackType.Single)
        {
            SpawnBullet(firePos.position, firePos.rotation);
            currentAmmo--;
        }
        else if (stats.attackType == WeaponAttackType.Multi)
        {
            float angleStep = 15f;  // 多发子弹间隔角度
            float startAngle = -angleStep * (stats.multiBulletCount - 1) / 2f;

            for (int i = 0; i < stats.multiBulletCount; i++)
            {
                Quaternion rotation = firePos.rotation * Quaternion.Euler(0, 0, startAngle + i * angleStep);
                SpawnBullet(firePos.position, rotation);
            }

            currentAmmo--;
        }
    }

    void SpawnBullet(Vector3 pos, Quaternion rot)
    {
        GameObject bulletObj = Instantiate(stats.bulletPrefab, pos, rot);
        // 可在这里设置子弹伤害或速度
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // bulletScript.damage = stats.damage; // 如果你的 Bullet 支持伤害设置
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(1f);  // 可改为 stats.reloadTime
        currentAmmo = stats.maxAmmo;
        isReloading = false;
    }

    // 获取当前弹药数量，可绑定 UI
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }
}