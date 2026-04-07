using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStats stats;
    private bool isDead = false;
    private bool isInvincible = false;


    #region 生命周期
    private void Awake()
    {
        if (stats == null) stats = GetComponent<PlayerStats>();
        
    }

    //为了确保血量初始化正确 将第一次发布事件的时间放在了 Start 而血条初始化放在了Awake 确保初始化正常
    public void Start()
    {
         EventBus.Instance.Trigger(new PlayerHealthChangedEventStruct { currentHealth = stats.CurrentHealth, maxHealth = stats.MaxHealth });
    }

    #endregion

    public void TakeDamage(int damage)
    {
        // 死亡或无敌时不受伤害
        if (isDead || isInvincible) return;

        // 通过 stats 修改血量，它会自动触发 HealthChangedEvent
        stats.ModifyHealth(-damage);

        // 检查是否死亡
        if (stats.CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 只有存活时才进入无敌（避免死亡后还无敌）
            StartCoroutine(InvincibilityCoroutine(1f));
            // 触发受伤事件（供动画、UI等使用）
            EventBus.Instance.Trigger(new PlayerHealthChangedEventStruct { currentHealth = stats.CurrentHealth ,maxHealth = stats.MaxHealth}); 
        }
    }

    private void Die()
    {
        isDead = true;

        EventBus.Instance.Trigger(new PlayerHealthChangedEventStruct { currentHealth = stats.CurrentHealth, maxHealth = stats.MaxHealth });
        // 发布死亡事件，让 PlayerController 或其他模块响应

        EventBus.Instance.Trigger(new PlayerDiedEvent());
        // 可以播放死亡动画、禁用输入等（由监听者处理）
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    // 可选：提供复活方法
    public void Revive(float health)
    {
        isDead = false;
        stats.ModifyHealth((int)health);
    }
}