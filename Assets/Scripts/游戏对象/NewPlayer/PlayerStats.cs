using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerStats : MonoBehaviour
{
    [Header("基础属性")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attack = 10;
    [SerializeField] private float moveSpeed = 5f;

    [Header("谐律能量")]
    [SerializeField] private float harmonyEnergy = 0f;  // 范围 0-1

    // 公开的只读属性
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int Attack => attack;
    public float MoveSpeed => moveSpeed;
    public float HarmonyEnergy => harmonyEnergy;

    private void Awake()
    {
        currentHealth = maxHealth;
        harmonyEnergy = 0f;
    }

    // ---------- 修改方法（发布事件） ----------
    public void ModifyHealth(int delta)
    {
        int oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + delta, 0, maxHealth);
        if (oldHealth != currentHealth)
        {
            EventBus.Instance.Trigger(new PlayerHealthChangedEvent(currentHealth, maxHealth));
            if (currentHealth <= 0)
                EventBus.Instance.Trigger(new PlayerDiedEvent());
        }
    }

    public void ModifyAttack(int delta)
    {
        attack += delta;
        EventBus.Instance.Trigger(new PlayerStatChangedEvent("Attack", attack));
    }

    public void ModifyMoveSpeed(float delta)
    {
        moveSpeed += delta;
        EventBus.Instance.Trigger(new PlayerStatChangedEvent("MoveSpeed", moveSpeed));
    }

    public void AddHarmony(float amount)
    {
        float old = harmonyEnergy;
        harmonyEnergy = Mathf.Clamp01(harmonyEnergy + amount);
        if (Mathf.Abs(old - harmonyEnergy) > 0.001f)
        {
            EventBus.Instance.Trigger(new HarmonyChangedEvent(harmonyEnergy));
        }
    }

    public bool ConsumeHarmony(float amount)
    {
        if (harmonyEnergy >= amount)
        {
            harmonyEnergy -= amount;
            EventBus.Instance.Trigger(new HarmonyChangedEvent(harmonyEnergy));
            return true;
        }
        return false;
    }
}
