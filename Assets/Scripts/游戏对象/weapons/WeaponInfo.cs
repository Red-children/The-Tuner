using System.Collections;
using System.Security.Cryptography;
using UnityEngine;




public class WeaponInfo : MonoBehaviour
{
    public int ID;

    [Header("切换设置")]
    public float switchCooldown = 0.5f;   // 切换冷却时间（秒）
    private float lastSwitchTime = -Mathf.Infinity;
    private bool isSwitching = false;
    private WeaponType pendingWeapon;      // 正在切换的目标武器

    [Header("Weapon Display Info")]
    public string weaponName;
    public string obtainMethod;
    public Sprite weaponSprite;

    [Header("Weapon Data")]
    public WeaponBase weaponBase;       // 数据库
    public WeaponType weaponType;       // 当前武器
    public Transform firePos;           // 射击位置

    public WeaponStats stats;           //当前武器状态数据    
    private int currentAmmo;            
    private float lastFireTime;
    private bool isReloading;

    [Header("武器所有者")]
    public BulletOwner owner; // 在 Inspector 中设置，玩家武器设为 Player，敌人武器设为 Enemy


    public RhythmData nowRhythmData; // 当前节奏数据
    public float ownerDamage = 0; // 由持有者传入的基础攻击力（敌人用）


    void Start()
    {
        //实例化武器
        InitializeWeapon(weaponType);
        nowRhythmData = new RhythmData(false,RhythmRank.Miss,1); // 默认不在窗口，倍率1
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
        EventBus.Instance.Trigger(new ChangeWeaponStruct(1, stats.id));
    }

    void Update()
    {
        
    }

    #region 射击方法
    public void Shoot()
    {
        if (isSwitching) return;           // 切换期间不能开火
        if (isReloading) return;           // 正在换弹时不能开枪

        if (currentAmmo <= 0)
        {
            // 没子弹了，自动换弹
            StartCoroutine(Reload());
            return;
        }

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

        // 触发开火事件
        EventBus.Instance.Trigger(new PlayerFireEvent
        {
            isPerfect = nowRhythmData.rank == RhythmRank.Perfect, // 根据需要调整
            rank = nowRhythmData.rank
        });

        // 触发开火事件（空结构体）
        EventBus.Instance.Trigger(new PlayerFiredEvent());

        //触发命名振动
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = stats.shakeIntensity });

        // 触发节奏命中事件（携带当前节奏数据）
        EventBus.Instance.Trigger(new RhythmHitEvent
        {
            rank = nowRhythmData.rank,
            intensity = nowRhythmData.rank == RhythmRank.Perfect ? 1f :
                        nowRhythmData.rank == RhythmRank.Great ? 0.6f : 0.3f
        });

        double now = AudioSettings.dspTime;
        double nextBeat = RhythmManager.Instance.GetNextBeatTime();
        double timeToNext = nextBeat - now;
        Debug.Log($"[Shoot] 玩家开火 at {now:F8}, 下一拍 at {nextBeat:F8}, 时间差 {timeToNext:F8}, 判定等级 {nowRhythmData.rank}, 是否在窗口 {nowRhythmData.isInWindow}");

    }
    #endregion

    #region 实例化子弹
    void SpawnBullet(Vector3 pos, Quaternion rot)
    {
        GameObject bulletObj = Instantiate(stats.bulletPrefab, pos, rot);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.owner = this.owner; // 传递所有者
                                         // 伤害计算（可根据所有者调整）
        float multiplier = (float)nowRhythmData.multiplier;
        float baseDamage = owner == BulletOwner.Player ? (ownerDamage + stats.damage) : (stats.damage); // 敌人子弹可能不需要玩家攻击力
        bulletScript.damage = baseDamage * multiplier;
        // ... 其他初始化 ...
    }
    #endregion

    #region 换弹冷却协程
    public IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(stats.reloadTime);
        currentAmmo = stats.maxAmmo;
        isReloading = false;
    }
    #endregion

    // 获取当前弹药数量，可绑定 UI
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    #region 实例化武器
    public void InitializeWeapon(WeaponType type)
    {
        stats = weaponBase.GetWeaponStats(type);
        if (stats == null)
        {
            Debug.LogError($"武器类型 {type} 未在 WeaponBase 中配置！");
            return;
        }
        currentAmmo = stats.maxAmmo;
        lastFireTime = -stats.fireRate;   // 重置冷却，使新武器可立即开火
        isReloading = false;
    }

    #endregion

    #region 切换武器
    public void SwitchWeapon(WeaponType newType)
    {
        if (newType == weaponType) return;          // 相同武器不切换
        if (isSwitching) return;                     // 正在切换中，不响应

        //是否在踩点窗口内，决定切换方式
        bool inRhythm = nowRhythmData.isInWindow;

        weaponType = newType;
        //下面这一行涉及手感问题 先弃用
        //InitializeWeapon(newType);        // 每次切换都重新初始化


        if (inRhythm)
        {
            // 完美切换：立即完成
            CompleteSwitch(newType);
        }
        else
        {
            // 普通切换：进入冷却
            isSwitching = true;
            pendingWeapon = newType;
            StartCoroutine(SwitchCooldownCoroutine());
        }
    }
    #endregion

    #region 切枪冷却协程
    // 切换冷却协程
    private IEnumerator SwitchCooldownCoroutine()
    {
        yield return new WaitForSeconds(switchCooldown);
        CompleteSwitch(pendingWeapon);
    }
    #endregion

    #region 完美切换
    //完美切换 立即完成切换
    private void CompleteSwitch(WeaponType newType)
    {
        // 记录旧武器的 ID
        int oldID = (stats != null) ? stats.id : -1;

        weaponType = newType;
        InitializeWeapon(newType);   // 重置弹药、开火冷却
        lastSwitchTime = Time.time;
        isSwitching = false;

        // 发布切换武器事件，参数为旧ID和新ID
        EventBus.Instance.Trigger(new ChangeWeaponStruct(oldID, stats.id));
    }
    #endregion

    #region 取消节奏数据订阅
    void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
    }
    #endregion

    #region 订阅节奏数据
    private void OnRhythmData(RhythmData data)
    {
        nowRhythmData = data;
    }
    #endregion

}


