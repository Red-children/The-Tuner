
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct PlayerHitEvent
{
    public bool isCritical;   // 是否精准命中
    public float damage;       // 伤害值（可选）
}

public struct PlayerMeleeEvent
{
    public float damage;
    public Vector2 hitPoint;
}

public struct CameraShakeEvent
{
    public float intensity;   // 震屏强度，可以根据伤害值决定
}




public class PlayerIObject : BaseObject
{
    private bool isStunned = false;   // 受击硬直
    private bool isDead = false;      // 死亡标志

    public  Animator animator;

    [Header("闪避设置")]
    public float dashDistance = 3f;          // 最大闪避距离
    public float dashDuration = 0.3f;        // 闪避持续时间
    
    public bool isDashing = false;             // 是否正在闪避
    public AnimationCurve dashCurve;              // 闪避位移曲线（可选，用于控制闪避的加速/减速效果）
    
    public float maxDashEnergy = 2;          // 闪避条上限
    public float currentDashEnergy = 2;   // 当前闪避条
    public float dashEnergyRegenRate = 1f;    // 闪避条恢复速率（每秒恢复多少）
    public bool isDashOnWindow = false;             // 是否在节奏窗口内可以闪避

    [Header("近战攻击设置")]
    public float meleeRange = 1.5f;          // 近战范围
    public LayerMask enemyLayer;              // 敌人层级
    public float meleeBaseDamage = 20f;       // 基础伤害
    public float meleeCooldown = 0.5f;        // 近战冷却
    private float lastMeleeTime = -999f;
    private float rhythmMultiplier = 1f; // 默认倍率1
    


    [Header("Weapon")]
    public WeaponInfo currentWeapon;   // 当前使用的武器
    
    public List<WeaponStats> weaponInfos;   // 武器数据列表（从 WeaponBase 获取）

    public bool isInvincible { get; private set; }  // 是否无敌

    private float invincibleTimer;   //无敌计时器


    //主摄像机 负责追踪玩家位置
    public Camera playerCamera;
    //墙的层级
    LayerMask wallLayer;

    //摄像头取点的位置
    public float offsetFactor = 0.3f;

    //相机移动的平滑度
    public float cameraSmoothness = 5f;
    //相机的z轴位置，确保相机在玩家前面
    public float cameraZ = -10f;

    //玩家的SpriteRenderer组件 用于翻转角色
    public SpriteRenderer spriteRenderer;

    public void Start()
    {
        animator = GetComponent<Animator>();
        //开始发布一次事件 让UI能正确显示初始血量
        EventBus.Instance.Trigger<PlayerHealthChangedEventStruct>(new PlayerHealthChangedEventStruct
        {
            currentHealth = nowHp,
            maxHealth = maxHp
        }); 

        #region 初始化
        //得到墙的层级 用来优化玩家碰撞
        wallLayer = LayerMask.GetMask("Wall");
        //调整当前相机的景深
        cameraZ = playerCamera.transform.position.z;
        //传递玩家攻击力到当前武器
        EventBus.Instance.Trigger(new PlayerAtkChange { atk = this.atk });
        // 从当前武器绑定的 WeaponBase 中获取数据列表
        weaponInfos = currentWeapon.weaponBase.weaponList;
        // 初始化武器数据
        currentWeapon.InitializeWeapon(currentWeapon.weaponType);
        #endregion

    }

    private void OnEnable()
    {
        
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
    }
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
    }
    private void OnRhythmData(RhythmData data)
    {
        rhythmMultiplier = (float)data.multiplier;
        isDashOnWindow = data.isInWindow; // 根据节奏窗口状态更新闪避可用性
    }

    

    #region 重写受伤方法


    // 重写 Wound 方法  传入伤害数值
    public override void Wound(int damage)
    {
        if (isInvincible || isStunned || isDead || nowHp <= 0) return;
        if (isInvincible || nowHp <= 0) { if (isInvincible) Debug.Log("玩家无敌，伤害被忽略"); return; }  // 无敌或已死亡则不处理

        // 扣血
        nowHp -= Mathf.Max(damage, 0);
        Debug.Log($"玩家受伤，当前血量: {nowHp}");

        EventBus.Instance.Trigger<PlayerHealthChangedEventStruct>(new PlayerHealthChangedEventStruct
        {
            currentHealth = nowHp,
            maxHealth = maxHp
        });

        // 触发震屏事件，强度可以简单设为 damage / 10f（根据你的数值调整）
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = damage / 100f });

        // 开启无敌帧
        StartCoroutine(InvincibilityCoroutine(1f)); // 无敌1秒

        if (nowHp <= 0)
        {
            nowHp = 0;
            Died();  // 调用自己的死亡方法（可以是重写的 Died）
        }
        StartCoroutine(StunCoroutine());
    }
    #endregion  

    #region 无敌帧的协程函数


    // 无敌协程 传入持续时间
    private IEnumerator InvincibilityCoroutine(float duration)
    {
        // 开始无敌
        isInvincible = true;

        //时间计数器
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 每0.1秒闪烁一次（示例）
            // 这里可以设置 SpriteRenderer 的透明度或颜色
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        isInvincible = false;
    }
    #endregion

    #region 重写死亡方法 发布玩家死亡事件


    public override void Died()
    {
        // 原有的死亡逻辑（播放特效、触发事件等）...
        Debug.Log("玩家死亡");

        // 新增：启动协程，2秒后重载场景
        StartCoroutine(RestartAfterDelay(2f));
    }

    private System.Collections.IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 重载当前场景（注意场景名要和你演示用的场景一致）
        SceneManager.LoadScene(0);
    }
    #endregion

    #region 玩家血量变化事件
    public void PlayerHpChange(PlayerHealthChangedEventStruct playerHealthChangedEventStruct)
    {
       playerHealthChangedEventStruct.currentHealth = nowHp;
       playerHealthChangedEventStruct.maxHealth = maxHp;
    }
    #endregion

   

    #region 近战攻击方法 计算伤害并检测敌人
    private void MeleeAttack()
    {
        lastMeleeTime = Time.time;

        // 计算最终伤害
        float finalDamage = (atk + meleeBaseDamage) * rhythmMultiplier;

        // 检测敌人
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, meleeRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.Wound(finalDamage);
            }
        }

        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = finalDamage * 0.1f }); // 示例强度

        // 触发近战事件（供音效等使用）
        EventBus.Instance.Trigger(new PlayerMeleeEvent { damage = finalDamage, hitPoint = transform.position });

    }
    #endregion

    public void Update()
    {
        if (isDead) return;                  // 死亡后什么都不做
        if (isStunned) return;               // 硬直中，禁止所有操作


        #region 移动逻辑
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        float rayLengthX = 0.9f; // 略大于玩家半径
        float rayLengthY = 0.9f;
        wallLayer = LayerMask.GetMask("Wall");

        // 分别检测X和Y方向，避免对角线同时被锁
        if (moveX != 0)
        {
            RaycastHit2D hitX = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(moveX), rayLengthX, wallLayer);
            if (hitX.collider != null) moveX = 0;
        }
        if (moveY != 0)
        {
            RaycastHit2D hitY = Physics2D.Raycast(transform.position, Vector2.up * Mathf.Sign(moveY), rayLengthY, wallLayer);
            if (hitY.collider != null) moveY = 0;
        }

        // 应用移动
        transform.Translate(new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime, Space.World);


        #endregion

        #region 开火检测

        //开火点空置检测


        if (Input.GetMouseButton(0))
            currentWeapon.Shoot();
        // 切换武器（数字键）
        if (Input.GetKeyDown(KeyCode.Alpha1))
            currentWeapon.SwitchWeapon(WeaponType.Pistol);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            currentWeapon.SwitchWeapon(WeaponType.Shotgun);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            currentWeapon.SwitchWeapon(WeaponType.Rifle);
        #endregion

        #region 鼠标追踪逻辑
        // 获取鼠标在世界空间中的位置（注意：ScreenToWorldPoint 需要正确的Z值）
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z; // 使用角色的屏幕深度
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // 计算向量（从角色指向鼠标）
        Vector2 directionMouse = mouseWorldPos - transform.position;

        //// 如果鼠标和角色重合，不旋转
        //if (directionMouse.magnitude > 0.01f)
        //{
        //    // 计算方向与X轴的夹角（弧度），转为角度
        //    float angle = Mathf.Atan2(directionMouse.y, directionMouse.x) * Mathf.Rad2Deg;
        //    //转向
        //    transform.rotation = Quaternion.Euler(0, 0, angle);
        //}

        // 在鼠标追踪逻辑后添加：
        if (directionMouse.x > 0)
            spriteRenderer.flipX = false; // 朝右
        else if (directionMouse.x < 0)
            spriteRenderer.flipX = true;  // 朝左       
                            // 注意：这里仅根据X轴方向决定翻转，如果鼠标在正上方，会保持上次朝向，但通常够用。
                                          // 更精细的可以结合方向角度，但先这样。
        #endregion

        #region 近战攻击检测
        if (Input.GetKeyDown(KeyCode.V) && Time.time > lastMeleeTime + meleeCooldown)
        {
            MeleeAttack();
        }
        #endregion


        // 武器指向鼠标
        if (currentWeapon != null)
        {
            Vector2 weaponDir = directionMouse; // 方向与玩家到鼠标一致
            float weaponAngle = Mathf.Atan2(weaponDir.y, weaponDir.x) * Mathf.Rad2Deg;
            currentWeapon.transform.rotation = Quaternion.Euler(0, 0, weaponAngle);
        }


        #region 武器切换
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeapon.SwitchWeapon(WeaponType.Pistol);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon.SwitchWeapon(WeaponType.Shotgun);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentWeapon.SwitchWeapon(WeaponType.Rifle);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            var weaponList = currentWeapon.weaponBase.weaponList;
            int count = weaponList.Count;
            if (count == 0) return; // 安全防护

            // 通过当前武器的类型查找在列表中的索引
            int currentIndex = weaponList.FindIndex(w => w.weaponType == currentWeapon.weaponType);
            if (currentIndex == -1)
            {
                Debug.LogWarning("当前武器类型不在武器列表中，默认切换到第一个");
                currentIndex = 0;
            }

            int delta = scroll > 0 ? 1 : -1;
            // (currentIndex + delta + count) % count 保证结果在 [0, count-1] 之间
            int newIndex = (currentIndex + delta + count) % count;

            WeaponType newType = weaponList[newIndex].weaponType;
            currentWeapon.SwitchWeapon(newType);
        }
        #endregion

        #region  节奏闪避
        if (currentDashEnergy < maxDashEnergy)
        {
            currentDashEnergy += dashEnergyRegenRate * Time.deltaTime;
            if (currentDashEnergy > maxDashEnergy)
                currentDashEnergy = maxDashEnergy;
        }

        // 2. 决定闪避方向
        Vector2 dashDir;
        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f)
        {
            // 有移动输入：使用WASD方向（归一化）
            dashDir = new Vector2(moveX, moveY).normalized;
        }
        else
        {
            // 无移动输入：使用鼠标方向
            dashDir = directionMouse.normalized; // directionMouse 已在前面计算
        }
        // 3. 计算闪避目标点（考虑墙壁）
        Vector3 targetPos = transform.position + (Vector3)dashDir * dashDistance;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDir, dashDistance, wallLayer);
        if (hit.collider != null)
        {
            Vector2 adjustedPoint = hit.point - dashDir * 0.2f;
            targetPos = new Vector3(adjustedPoint.x, adjustedPoint.y, transform.position.z);
        }

        // 4. 触发闪避
        if (Input.GetMouseButtonDown(1) && (currentDashEnergy > 1 || isDashOnWindow))
        {
            if (!isDashOnWindow)
            {
                currentDashEnergy -= 1;
            }
            StartCoroutine(DashCoroutine(transform.position, targetPos, dashDuration));
        }
        #endregion


    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }

    private IEnumerator DashCoroutine(Vector3 start, Vector3 target, float duration)
    {
        isDashing = true;
        isInvincible = true;

         
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveT = dashCurve.Evaluate(t);
            transform.position = Vector3.Lerp(start, target, curveT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target; // 确保最终位置准确

        isDashing = false;
        isInvincible = false;
    }

    private IEnumerator StunCoroutine()
    {
        isStunned = true;
        // 播放受击动画（假设有 Animator）
        //animator?.SetTrigger("Hurt");

        // 无敌协程已经存在，可以复用或单独处理
        // 这里仅处理硬直时间
        yield return new WaitForSeconds(0.2f); // 硬直时间
        isStunned = false;
    }
    

}



    

