using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("基础属性")]
    public float maxHealth = 100f;
    protected float currentHealth;

    [Header("通用引用")]
    public Transform target;          // 当前目标（玩家）

    // 事件：用于通知房间
    public System.Action<BaseEnemy> OnDeath;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    // 设置目标
    public virtual void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // 受伤方法
    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    // 死亡方法
    protected virtual void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}