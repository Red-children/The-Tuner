using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    
    [Header("玩家状态")]
    [SerializeField] private bool isInvincible = false;
    [SerializeField] private float invincibilityTimer = 0f;
    
    // 组件引用
    private PlayerHealth playerHealth;
    private SpriteRenderer playerSprite;
    private PlayerController playerController;
    
    // 事件
    public event System.Action<bool> OnInvincibilityChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // 查找玩家对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerSprite = player.GetComponent<SpriteRenderer>();
            playerController = player.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("PlayerManager: 未找到玩家对象");
        }
    }
    
    private void Update()
    {
        // 更新无敌计时器
        if (isInvincible && invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // 无敌时间结束
            if (invincibilityTimer <= 0)
            {
                DisableInvincibility();
            }
        }
    }
    
    /// <summary>
    /// 启用无敌状态
    /// </summary>
    /// <param name="duration">无敌持续时间</param>
    public void EnableInvincibility(float duration)
    {
        if (playerHealth == null) return;
        
        isInvincible = true;
        invincibilityTimer = duration;
        
        // 设置玩家为无敌状态
        SetPlayerInvincible(true);
        
        // 开始闪烁效果
        StartCoroutine(InvincibilityFlashCoroutine());
        
        // 触发事件
        OnInvincibilityChanged?.Invoke(true);
        
        Debug.Log($"玩家进入无敌状态，持续时间: {duration}秒");
    }
    
    /// <summary>
    /// 禁用无敌状态
    /// </summary>
    public void DisableInvincibility()
    {
        if (playerHealth == null) return;
        
        isInvincible = false;
        invincibilityTimer = 0f;
        
        // 恢复玩家正常状态
        SetPlayerInvincible(false);
        
        // 停止闪烁效果
        StopAllCoroutines();
        if (playerSprite != null)
        {
            playerSprite.color = Color.white;
        }
        
        // 触发事件
        OnInvincibilityChanged?.Invoke(false);
        
        Debug.Log("玩家无敌状态结束");
    }
    
    /// <summary>
    /// 设置玩家无敌状态
    /// </summary>
    private void SetPlayerInvincible(bool invincible)
    {
        // 由于现有的PlayerHealth没有公开的SetInvincible方法，
        // 我们通过其他方式实现无敌效果
        
        // 方法1：通过修改玩家层级来避免碰撞
        if (playerController != null)
        {
            // 可以在这里添加避免碰撞的逻辑
            // 例如临时修改玩家层级
        }
        
        // 方法2：通过事件系统通知其他组件
        EventBus.Instance.Trigger(new PlayerInvincibilityEvent { IsInvincible = invincible });
    }
    
    /// <summary>
    /// 无敌状态闪烁效果
    /// </summary>
    private IEnumerator InvincibilityFlashCoroutine()
    {
        if (playerSprite == null) yield break;
        
        float flashInterval = 0.1f;
        while (isInvincible)
        {
            // 闪烁效果：半透明与不透明交替
            playerSprite.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(flashInterval);
            
            playerSprite.color = Color.white;
            yield return new WaitForSeconds(flashInterval);
        }
    }
    
    /// <summary>
    /// 检查玩家是否无敌
    /// </summary>
    public bool IsInvincible()
    {
        return isInvincible;
    }
    
    /// <summary>
    /// 获取剩余无敌时间
    /// </summary>
    public float GetRemainingInvincibilityTime()
    {
        return Mathf.Max(0f, invincibilityTimer);
    }
}

/// <summary>
/// 玩家无敌状态事件
/// </summary>
public struct PlayerInvincibilityEvent
{
    public bool IsInvincible { get; set; }
}