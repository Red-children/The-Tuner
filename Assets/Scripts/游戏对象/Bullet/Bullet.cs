using UnityEngine;

/// <summary>
/// 子弹类，处理子弹的移动、碰撞和穿透效果
/// </summary>
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
    [SerializeField] private float penetrationVisualOffset = 0.1f;   // 穿透时的视觉层级偏移

    private int layerMask;              // 检测子弹可碰撞的层级
    private const float STEP_DISTANCE = 0.1f; // 每帧移动的最大步长
    private Collider2D lastHitEnemy;    // 记录最后击中的敌人，避免重复碰撞
    private SpriteRenderer spriteRenderer; // 子弹的精灵渲染器，用于层级控制
    private int originalSortingOrder;   // 原始的排序层级
    public RhythmRank currentRhythmRank; // 当前节奏数据，用于卡肉感效果

    private void Start()
    {
        // 初始化子弹特效，如果未赋值则从 Resources 加载
        if (destroyEffect == null)
            destroyEffect = Resources.Load<GameObject>("Eff");

        // 获取精灵渲染器
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        // 根据子弹层级设置碰撞检测的层级掩码
        int bulletLayer = gameObject.layer;
        if (bulletLayer == LayerMask.NameToLayer("PlayerBullet"))
        {
            layerMask = LayerMask.GetMask("Enemy", "Wall" );
            
            // 检查连击管理器是否启用穿透效果
            if (ComboManager.Instance != null)
{
    canPenetrate = ComboManager.Instance.HasEffect(ComboEffect.BulletPenetration);
    if (canPenetrate)
    {
        Debug.Log("子弹启用穿透效果");
        // 直接改颜色
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 0.5f, 0.8f); // 粉紫色，你也可以在Inspector里配置
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
        // 创建射线检测参数，忽略最后击中的敌人
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, stepDistance, layerMask);
        
        // 如果检测到碰撞且不是最后击中的敌人，处理碰撞
        if (hit.collider != null && hit.collider != lastHitEnemy)
        {
            return HandleHit(hit);
        }

        // 没有碰撞或碰撞的是最后击中的敌人，正常移动
        transform.Translate(transform.right * stepDistance, Space.World);
        return false;
    }
    #endregion

    /// <summary>
    /// 处理碰撞逻辑，返回 true 表示子弹应该被销毁
    /// </summary>
    private bool HandleHit(RaycastHit2D hit)
    {
        DoorActivator activator = hit.collider.GetComponent<DoorActivator>();
        if (activator != null)
        {
            activator.TakeHit(currentRhythmRank);
            DestroyMyself();
            return true;
        }


        // 玩家子弹击中敌人
        if (gameObject.layer == LayerMask.NameToLayer("PlayerBullet") && hit.collider.CompareTag("Enemy"))
        {
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();




            if (enemy != null)
            {
                // 计算伤害（考虑穿透衰减）
                float finalDamage = CalculatePenetrationDamage(damage);
               enemy.Wound(finalDamage, currentRhythmRank);
                // 触发敌人被命中的事件
                EventBus.Instance.Trigger(new EnemyHitEvent(1, currentRhythmRank));
                Debug.Log ($"子弹击中敌人，造成 {finalDamage} 伤害，当前节奏判定：{currentRhythmRank}");

                // 触发敌人卡肉感效果
                if (HitStopManager.Instance != null)
                {
                    // 直接使用缓存的节奏Rank，避免时间差问题
                    RhythmRank rank = currentRhythmRank; // 使用发射时缓存的Rank

                    HitStopManager.Instance.TriggerEnemyHitStop(hit.collider.gameObject, rank, finalDamage);

                    // 添加调试信息，确认使用缓存Rank
                    Debug.Log($"[Bullet] 卡肉感使用缓存Rank: {rank} (发射时判定)");
                }
                
                // 处理穿透逻辑
                if (canPenetrate)
                {
                    //增加穿透计数器
                    currentPenetrationCount++;
                    
                    // 记录最后击中的敌人，避免重复碰撞
                    lastHitEnemy = hit.collider;
                    
                    // 调整视觉层级，让子弹显示在敌人前面
                    AdjustVisualLayerForPenetration();
                    
                    // 检查是否达到最大穿透次数
                    if (currentPenetrationCount >= maxPenetrationCount)
                    {
                        DestroyMyself();
                        return true;
                    }
                    else
                    {
                        // 继续正常移动，通过层级控制实现穿透效果
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
    /// 调整视觉层级，实现穿透效果
    /// </summary>
    private void AdjustVisualLayerForPenetration()
    {
        if (spriteRenderer != null)
        {
            // 提高子弹的排序层级，让它显示在敌人前面
            spriteRenderer.sortingOrder = originalSortingOrder + 10;
            
            // 可以添加一些视觉特效，比如半透明效果
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.8f);
            
            Debug.Log("子弹调整视觉层级，实现穿透效果");
        }
    }
    
    /// <summary>
    /// 恢复原始视觉层级
    /// </summary>
    private void RestoreVisualLayer()
    {
        if (spriteRenderer != null)
        {
            // 恢复原始排序层级
            spriteRenderer.sortingOrder = originalSortingOrder;
            
            // 恢复原始颜色
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }
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