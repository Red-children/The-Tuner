using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerIObject : BaseObject
{

    //主摄像机 负责追踪玩家位置
    public Camera camera;

    //子弹数据后期转移至武器系统
    public BulletData data;

    //开火点位置，在层级窗口中拖动到枪口对应的位置
    public Transform FirePos;

    //开火间隔时间 记得后面转武器然后用配置文件来决定开火间隔时间
    public float fireInterval = 0.5f;
    //计时器
    public float fireTimer = 0f;

    // 用于存放子弹预制体，可在Inspector拖拽或Resources加载
    //子弹对象，后期记得挂载到武器上面，由武器决定发射子弹的类型,此处仅做测试
    public Bullet bulletPrefab;

    //当前屏幕上的点
    public Vector3 nowPos;
    //上一次玩家的位置 就是在位移前 玩家的位置
    public Vector3 frontPos;

    //墙的层级
    //LayerMask wallLayer = LayerMask.GetMask("Wall");

    //摄像头取点的位置
    public float offsetFactor = 0.3f;

    //相机移动的平滑度
    public float cameraSmoothness = 5f;

    //玩家的Z轴位置，确保相机在玩家前面
    public float z = 0;

    //相机的z轴位置，确保相机在玩家前面
    public float cameraZ = -10f;

    public override void Fire()
    {
        Bullet newBullet = Instantiate(bulletPrefab, FirePos.position, FirePos.rotation);
        newBullet.transform.position = FirePos.position;
        newBullet.transform.rotation = FirePos.rotation;
    }


    public void Start()
    {
        
        #region 子弹加载测试 后面用配置文件来决定子弹类型当前为硬编码
        data = new BulletData();
        data.resPath = "Bullet";
        #endregion

        #region 初始化
        //得到墙的层级 用来优化玩家碰撞
        LayerMask wallLayer = LayerMask.GetMask("Wall");
        //动态加载子弹预制体，后续改为武器系统来决定加载哪个子弹预制体
        bulletPrefab = Resources.Load<Bullet>("Bullet") as Bullet;
        //调整当前相机的景深
        cameraZ = camera.transform.position.z;
        #endregion

    }

    public void Update()
    {
        #region 移动逻辑
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        float rayLengthX = 0.9f; // 略大于玩家半径
        float rayLengthY = 0.9f;
        LayerMask wallLayer = LayerMask.GetMask("Wall");

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
        if (FirePos != null && bulletPrefab != null && Input.GetMouseButtonDown(0) )
        {
            Fire();
        }

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
        camera.transform.position =  Vector3.Lerp(camera.transform.position, targetCameraPos, Time.deltaTime * cameraSmoothness);
        #endregion

    }
}
