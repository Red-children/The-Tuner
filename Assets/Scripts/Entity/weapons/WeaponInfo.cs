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

public abstract class WeaponInfo : MonoBehaviour
{
    [Header("武器配置")]
    public WeaponBase weaponBase;//武器基础数据
    public WeaponType weaponType;//武器类型
    public Transform firePoint;//射击点

    protected WeaponStats weaponStats;//武器统计数据
    public WeaponOwner owner;//武器所有者

    public int CurrentAmmo => currentAmmo;
    public int WeaponId => weaponStats?.id ?? -1;

    protected int currentAmmo;
    protected double lastShootTime;
    protected bool isReloading;

    protected virtual void Awake()
    {
        if (weaponBase == null)
        {
            Debug.LogError($"[WeaponInfo] {gameObject.name} weaponBase 为 null");
            return;
        }
        weaponStats = weaponBase.GetWeaponStats(weaponType);
        if (weaponStats == null)
        {
            Debug.LogError($"[WeaponInfo] {gameObject.name} 未找到 {weaponType} 对应的数据");
            return;
        }
        currentAmmo = weaponStats.maxAmmo;
    }
        
    /// <summary>
    /// 处理武器输入
    /// </summary>
    public virtual void HandleFireInput() { }

    /// <summary>
    /// 射击武器
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="rhythmMultiplier">节奏乘数</param>
    /// <param name="rank">节奏等级</param>
    public virtual void Fire(float damage, float rhythmMultiplier, RhythmRank rank) { }

    /// <summary>
    /// 武器开火
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="rhythmMultiplier">节奏乘数</param>
    /// <param name="rank">节奏等级</param>
    public void Shoot(float damage, float rhythmMultiplier, RhythmRank rank)
    {
        Fire(damage, rhythmMultiplier, rank);
    }

    public int GetCurrentAmmo() => currentAmmo;

    /// <summary>
    /// 生成子弹
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="rank">节奏等级</param>
    protected void SpawnBullet(float damage, RhythmRank rank)
    {
        GameObject bulletObj = Instantiate(weaponStats.bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.currentRhythmRank = rank;
        bullet.SetDamage(damage);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            bullet.SetAttackerPosition(player.transform.position);
    }

    /// <summary>
    /// 开始重新加载
    /// </summary>
    protected void StartReload()
    {
        if (isReloading) return;
        StartCoroutine(ReloadCoroutine());
    }

    /// <summary>
    /// 重新加载武器
    /// </summary>
    /// <returns>IEnumerator</returns>
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
}