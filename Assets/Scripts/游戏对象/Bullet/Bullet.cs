using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("移动参数")]
    public int moveSpeed = 10;
    public float damage = 10f;
    public GameObject destroyEffect;   // 子弹销毁特效预制体

    [Header("穿透效果")]
    [SerializeField] private bool canPenetrate = false;        // 是否可穿透
    [SerializeField] private int maxPenetrationCount = 3;       // 最大穿透次数
    [SerializeField] private int currentPenetrationCount = 0;    // 当前穿透次数
    [SerializeField] private float penetrationDamageReduction = 0.2f; // 穿透伤害衰减

    private int layerMask;              // 检测子弹可碰撞的层级
    private const float STEP_DISTANCE = 0.1f; // 每帧移动的最大步长

    private void Start()
    {
        // 初始化子弹特效，如果未赋值则从 Resources 加载
        if (destroyEffect == null)
            destroyEffect = Resources.Load<GameObject>("Eff");

        // 根据子弹层级设置碰撞检测的层级掩码
        int bulletLayer = gameObject.layer;
        if (bulletLayer == LayerMask.NameToLayer("PlayerBullet"))
        {
            layerMask = LayerMask.GetMask("Enemy", "Wall");
            
            // 检查连击管理器是否启用穿透效果
            if (ComboManager.Instance != null)
            {
                canPenetrate = ComboManager.Instance.HasEffect(ComboEffect.BulletPenetration);
                if (canPenetrate)
                {
                    Debug.Log("子弹启用穿透效果");
                }
            }
        }
        else if (bulletLayer == LayerMask.NameToLayer("EnemyBullet"))
        {
            layerMask = LayerMask.GetMask("Player", "Wall");
        }
        else
        {
            Debug.LogError($"子弹 {name} 的层级未设置为 PlayerBullet 或 EnemyBullet，无法正确检测碰撞。");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        float moveDistance = Time.deltaTime * moveSpeed;
        int steps = Mathf.CeilToInt(moveDistance / STEP_DISTANCE);
        float step = moveDistance / steps;
        
        // 分步移动，每步检测碰撞
        for (int i = 0; i < steps; i++)
        {
            // 执行一步移动并检测碰撞
            if (MoveStep(step))
                return; // 如果发生碰撞且需要销毁，退出 Update
        }
    }

    #region 移动和碰撞检测
    private bool MoveStep(float stepDistance)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, stepDistance, layerMask);
        if (hit.collider != null)
        {
            return HandleHit(hit);
        }

        // 没有碰撞，正常移动
        transform.Translate(transform.right * stepDistance, Space.World);
        return false;
    }
    #endregion

    /// <summary>
    /// 处理碰撞逻辑，返回 true 表示子弹应该被销毁
    /// </summary>
    private bool HandleHit(RaycastHit2D hit)
    {
        // 玩家子弹击中敌人
        if (gameObject.layer == LayerMask.NameToLayer("PlayerBullet") && hit.collider.CompareTag("Enemy"))
        {
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // 计算伤害（考虑穿透衰减）
                float finalDamage = CalculatePenetrationDamage(damage);
                enemy.Wound(finalDamage);
                
                // 处理穿透逻辑
                if (canPenetrate)
                {
                    currentPenetrationCount++;
                    
                    // 检查是否达到最大穿透次数
                    if (currentPenetrationCount >= maxPenetrationCount)
                    {
                        DestroyMyself();
                        return true;
                    }
                    else
                    {
                        // 继续移动，不销毁子弹
                        transform.Translate(transform.right * 0.1f, Space.World); // 稍微移动避免重复碰撞
                        return false;
                    }
                }
                else
                {
                    // 没有穿透效果，正常销毁
                    DestroyMyself();
                    return true;
                }
            }
        }
        // 敌人子弹击中玩家
        else if (gameObject.layer == LayerMask.NameToLayer("EnemyBullet") && hit.collider.CompareTag("Player"))
        {
            PlayerAPI player = hit.collider.GetComponent<PlayerAPI>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
                DestroyMyself();
                return true;
            }
        }
        // 子弹击中墙壁
        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // 墙壁碰撞直接销毁
            DestroyMyself();
            return true;
        }
        else
        {
            // 其他情况，不处理碰撞
            return false;
        }

        return false;
    }

    /// <summary>
    /// 计算穿透后的伤害
    /// </summary>
    private float CalculatePenetrationDamage(float baseDamage)
    {
        if (!canPenetrate || currentPenetrationCount == 0)
        {
            return baseDamage;
        }
        
        // 伤害衰减公式：每次穿透减少一定百分比伤害
        float damageMultiplier = Mathf.Pow(1f - penetrationDamageReduction, currentPenetrationCount);
        return baseDamage * damageMultiplier;
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

    /// <summary>
    /// 设置子弹伤害
    /// </summary>
    public void SetDamage(float damage) 
    {
        this.damage = Mathf.Max(0, damage);
    }

    /// <summary>
    /// 启用穿透效果（可由外部调用）
    /// </summary>
    public void EnablePenetration(int maxPenetrations = 3, float damageReduction = 0.2f)
    {
        canPenetrate = true;
        maxPenetrationCount = maxPenetrations;
        penetrationDamageReduction = damageReduction;
        currentPenetrationCount = 0;
    }

    /// <summary>
    /// 禁用穿透效果
    /// </summary>
    public void DisablePenetration()
    {
        canPenetrate = false;
        currentPenetrationCount = 0;
    }
}