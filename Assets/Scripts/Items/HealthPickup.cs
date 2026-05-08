using UnityEngine;

/// <summary>
/// 回血道具
/// 通过碰撞检测检测玩家，恢复玩家生命值
/// </summary>
public class HealthPickup : MonoBehaviour
{
    [Header("回血参数")]
    [Tooltip("恢复的血量")]
    public int healAmount = 10;

    [Tooltip("是否只在玩家受伤时生效")]
    public bool onlyWhenHurt = false;

    [Tooltip("拾取后是否销毁道具")]
    public bool destroyOnPickup = true;

    [Tooltip("拾取特效")]
    public GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测玩家标签
        if (other.CompareTag("Player"))
        {
            // 获取玩家API
            PlayerAPI playerAPI = other.GetComponent<PlayerAPI>();
            if (playerAPI != null && playerAPI.health != null)
            {
                // 获取玩家当前血量
                PlayerStats stats = playerAPI.health.GetComponent<PlayerStats>();
                if (stats == null)
                {
                    stats = playerAPI.health.GetComponentInParent<PlayerStats>();
                }

                if (stats != null)
                {
                    // 如果设置了只在受伤时生效，检查当前血量是否小于最大血量
                    if (onlyWhenHurt && stats.CurrentHealth >= stats.MaxHealth)
                    {
                        return;
                    }

                    // 使用 ModifyHealth 恢复血量（正数为恢复）
                    stats.ModifyHealth(healAmount);
                    Debug.Log($"[HealthPickup] 玩家拾取回血道具，恢复 {healAmount} 点血量");

                    // 播放拾取特效
                    if (pickupEffect != null)
                    {
                        Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    }

                    // 销毁道具
                    if (destroyOnPickup)
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    Debug.LogWarning($"[HealthPickup] 找不到玩家的 PlayerStats 组件");
                }
            }
            else
            {
                Debug.LogWarning($"[HealthPickup] 玩家没有 PlayerAPI 组件或 PlayerHealth 组件");
            }
        }
    }
}