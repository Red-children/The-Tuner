using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Jobs;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public struct PlayerHealthChangedEventStruct
{
    public float currentHealth;
    public float maxHealth;
    public float healthPercent => currentHealth / maxHealth; //方便UI直接使用
}



public struct PlayerDiedEvent
{
    public PlayerIObject player;
}


public class PlayerIObject : BaseObject
{
    

    [Header("Weapon")]
    public WeaponInfo currentWeapon;   // 当前使用的武器
    
    public List<WeaponStats> weaponInfos;   // 武器数据列表（从 WeaponBase 获取）

    public bool isInvincible { get; private set; }  // 是否无敌

    private float invincibleTimer;   //无敌计时器


    //主摄像机 负责追踪玩家位置
    public Camera playerCamera;


    //开火间隔时间 记得后面转武器然后用配置文件来决定开火间隔时间
    public float fireInterval = 0.5f;
    //计时器
    public float fireTimer = 0f;

  
    //当前屏幕上的点
    public Vector3 nowPos;
    //上一次玩家的位置 就是在位移前 玩家的位置
    public Vector3 frontPos;

    //墙的层级
    LayerMask wallLayer;

    //摄像头取点的位置
    public float offsetFactor = 0.3f;

    //相机移动的平滑度
    public float cameraSmoothness = 5f;

    //玩家的Z轴位置，确保相机在玩家前面
    public float z = 0;

    //相机的z轴位置，确保相机在玩家前面
    public float cameraZ = -10f;

  

    public void Start()
    {
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
        passPlayerAtk();
        // 从当前武器绑定的 WeaponBase 中获取数据列表
        weaponInfos = currentWeapon.weaponBase.weaponList;
        // 初始化武器数据
        currentWeapon.InitializeWeapon(currentWeapon.weaponType);
        #endregion

    }

    #region 重写受伤方法

    
    // 重写 Wound 方法  传入伤害数值
    public override void Wound(int damage)
    {

        if (isInvincible || nowHp <= 0) return;  // 无敌或已死亡则不处理

        // 扣血
        nowHp -= Mathf.Max(damage, 0);
        Debug.Log($"玩家受伤，当前血量: {nowHp}");

        EventBus.Instance.Trigger<PlayerHealthChangedEventStruct>(new PlayerHealthChangedEventStruct
        {
            currentHealth = nowHp,
            maxHealth = maxHp

        });

        // 开启无敌帧
        StartCoroutine(InvincibilityCoroutine(1f)); // 无敌1秒

        if (nowHp <= 0)
        {
            nowHp = 0;
            Died();  // 调用自己的死亡方法（可以是重写的 Died）
        }
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


    // 重写死亡方法（可选）
    public override void Died()
    {
       
        Debug.Log("玩家死亡");
        // 发布玩家死亡事件（供UI、音效等监听）
        EventBus.Instance.Trigger(new PlayerDiedEvent { player = this });
        // 播放死亡动画、销毁等（可暂不销毁，而是显示 GameOver 界面）
        base.Died();  // 会销毁对象，如果不想立即销毁，可以注释 base.Died()
    }
    #endregion

    #region 玩家血量变化事件
    public void PlayerHpChange(PlayerHealthChangedEventStruct playerHealthChangedEventStruct)
    {
       playerHealthChangedEventStruct.currentHealth = nowHp;
       playerHealthChangedEventStruct.maxHealth = maxHp;
    }
    #endregion

    #region 玩家传递攻击力到武器
    // 传递攻击力到当前武器
    public void passPlayerAtk()
    {
        currentWeapon.playerAtk = this.atk;
    }
    #endregion
    public void Update()
    {
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

        // 如果鼠标和角色重合，不旋转
        if (directionMouse.magnitude > 0.01f)
        {
            // 计算方向与X轴的夹角（弧度），转为角度
            float angle = Mathf.Atan2(directionMouse.y, directionMouse.x) * Mathf.Rad2Deg;
            //转向
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        #endregion

        #region 相机追踪逻辑
        //玩家和准星之间的向量上取一点 相机对这一点做线性插值
        Vector2 cameraOffset =  directionMouse * offsetFactor;
        Vector3 targetCameraPos = transform.position + new Vector3(cameraOffset.x, cameraOffset.y, cameraZ); // 确保相机在玩家前面
        playerCamera.transform.position =  Vector3.Lerp(playerCamera.transform.position, targetCameraPos, Time.deltaTime * cameraSmoothness);
        #endregion

        #region 武器切换
        if (Input.GetKeyDown(KeyCode.Alpha1))
            currentWeapon.SwitchWeapon(WeaponType.Pistol);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            currentWeapon.SwitchWeapon(WeaponType.Shotgun);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            currentWeapon.SwitchWeapon(WeaponType.Rifle);

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
    }

 
}



    

