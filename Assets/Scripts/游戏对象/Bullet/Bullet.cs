using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("移动参数")]
    public int moveSpeed = 10;
    public float damage = 10f;
    public GameObject destroyEffect;   // 子弹销毁特效预制体

    private int layerMask;              // 根据子弹层决定的检测掩码
    private const float STEP_DISTANCE = 0.1f; // 每次射线检测的最大步长

    private void Start()
    {
        // 初始化子弹特效（如果未赋值，从 Resources 加载）
        if (destroyEffect == null)
            destroyEffect = Resources.Load<GameObject>("Eff");

        // 根据子弹自身层设置射线检测的目标层
        int bulletLayer = gameObject.layer;
        if (bulletLayer == LayerMask.NameToLayer("PlayerBullet"))
        {
            layerMask = LayerMask.GetMask("Enemy", "Wall");
        }
        else if (bulletLayer == LayerMask.NameToLayer("EnemyBullet"))
        {
            layerMask = LayerMask.GetMask("Player", "Wall");
        }
        else
        {
            Debug.LogError($"子弹 {name} 的层未设置为 PlayerBullet 或 EnemyBullet，将被销毁。");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        float moveDistance = Time.deltaTime * moveSpeed;
        int steps = Mathf.CeilToInt(moveDistance / STEP_DISTANCE);
        float step = moveDistance / steps; // 实际每步移动距离

        for (int i = 0; i < steps; i++)
        {
            // 执行一步移动并检测碰撞
            if (MoveStep(step))
                return; // 如果命中并销毁，退出 Update
        }
    }

    #region
    private bool MoveStep(float stepDistance)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, stepDistance, layerMask);
        if (hit.collider != null)
        {
            return HandleHit(hit);
        }

        // 没有碰撞，继续移动
        transform.Translate(transform.right * stepDistance, Space.World);
        return false;
    }
    #endregion

    /// <summary>
    /// 处理命中逻辑，返回 true 表示子弹应销毁
    /// </summary>
    private bool HandleHit(RaycastHit2D hit)
    {
        

        // 玩家子弹击中敌人
        if (gameObject.layer == LayerMask.NameToLayer("PlayerBullet") && hit.collider.CompareTag("Enemy"))
        {
            hit.collider.GetComponent<EnemyController>()?.Wound(damage);
        }
        // 敌人子弹击中玩家
        else if (gameObject.layer == LayerMask.NameToLayer("EnemyBullet") && hit.collider.CompareTag("Player"))
        {
            hit.collider.GetComponent<PlayerIObject>()?.Wound((int)damage);
        }
        // 子弹击中墙壁
        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // 墙壁无特殊逻辑，直接销毁
            DestroyMyself();
        }
        else
        {
            // 意外命中（比如子弹打中同阵营？），根据层设计不应发生
            return false;
        }

        DestroyMyself();
        return true;
    }

    /// <summary>
    /// 销毁子弹并播放特效
    /// </summary>
    public void DestroyMyself()
    {
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    

}