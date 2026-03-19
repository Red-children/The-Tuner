using System.Collections;
using System.Security.Cryptography;
using UnityEngine;



public enum WeaponOwner
{
    Player,
    Enemy
}

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
    public WeaponOwner owner; // 在 Inspector 中设置，玩家武器设为 Player，敌人武器设为 Enemy


    public RhythmData nowRhythmData; // 当前节奏数据
    public float ownerDamage = 0; // 由持有者传入的基础攻击力（敌人用）

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<PlayerAtkChange>(OnPlayerAtkChange);
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<PlayerAtkChange > (OnPlayerAtkChange);
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
    }

    void Start()
    {
        //实例化武器
        InitializeWeapon(weaponType);
        nowRhythmData = new RhythmData(false,RhythmRank.Miss,1); // 默认不在窗口，倍率1
       
        EventBus.Instance.Trigger(new ChangeWeaponStruct(1, stats.id));
    }

    void Update()
    {
        
    }

    #region 射击方法
    public void Shoot()
    {
        if (isSwitching) return;
        if (isReloading) return;
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        if (Time.time - lastFireTime < stats.fireRate)
            return;

        lastFireTime = Time.time;

        // 实时判定
        double now = AudioSettings.dspTime;
        var rankResult = RhythmManager.Instance.GetRank(now);

        // 生成子弹（传入 multiplier）
        float multiplier = rankResult.multiplier;
        if (stats.attackType == WeaponAttackType.Single)
        {
            SpawnBullet(firePos.position, firePos.rotation, multiplier);
            currentAmmo--;
        }
        else if (stats.attackType == WeaponAttackType.Multi)
        {
            float angleStep = 15f;
            float startAngle = -angleStep * (stats.multiBulletCount - 1) / 2f;
            for (int i = 0; i < stats.multiBulletCount; i++)
            {
                Quaternion rotation = firePos.rotation * Quaternion.Euler(0, 0, startAngle + i * angleStep);
                SpawnBullet(firePos.position, rotation, multiplier);
            }
            currentAmmo--;
        }

        // 触发各种事件（使用实时等级）
        EventBus.Instance.Trigger(new PlayerFireEvent
        {
            isPerfect = rankResult.rank == RhythmRank.Perfect,
            rank = rankResult.rank
        });
        EventBus.Instance.Trigger(new PlayerFiredEvent());
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = stats.shakeIntensity });
        EventBus.Instance.Trigger(new RhythmHitEvent
        {
            rank = rankResult.rank,
            intensity = rankResult.rank == RhythmRank.Perfect ? 1f :
                        rankResult.rank == RhythmRank.Great ? 0.6f : 0.3f
        });

        // 日志（区分玩家/敌人）
        double nextBeat = RhythmManager.Instance.GetNextBeatTime();
        string ownerStr = owner == WeaponOwner.Player ? "玩家" : "敌人";
        Debug.Log($"[{ownerStr}开火] at {now:F8}, 下一拍 at {nextBeat:F8}, 判定等级 {rankResult.rank}, 是否在窗口 {rankResult.isInWindow}");
    }
    #endregion

    #region 实例化子弹
    void SpawnBullet(Vector3 pos, Quaternion rot, float multiplier)
    {
        GameObject bulletObj = Instantiate(stats.bulletPrefab, pos, rot);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();


        // 设置子弹层级（可选，配合射线检测的层掩码）
        if (owner == WeaponOwner.Player)
            bulletObj.layer = LayerMask.NameToLayer("PlayerBullet");
        else
            bulletObj.layer = LayerMask.NameToLayer("EnemyBullet");

        // 计算伤害
        float baseDamage = (owner == WeaponOwner.Player) ? (ownerDamage + stats.damage) : stats.damage;
        bulletScript.damage = baseDamage * multiplier;
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

    public void OnPlayerAtkChange(PlayerAtkChange playerAtkChange)
    {
        ownerDamage = playerAtkChange.atk;
    }


}




