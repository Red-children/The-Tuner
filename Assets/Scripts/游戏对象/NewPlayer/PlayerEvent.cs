
using UnityEngine;

public struct PlayerHealthChangedEvent
{
    public int currentHealth;
    public int maxHealth;
    public PlayerHealthChangedEvent(int cur, int max) { currentHealth = cur; maxHealth = max; }
}

public struct PlayerStatChangedEvent
{
    public string statName;
    public float newValue;
    public PlayerStatChangedEvent(string name, float val) { statName = name; newValue = val; }
}

public struct HarmonyChangedEvent
{
    public float harmony;
    public HarmonyChangedEvent(float h) { harmony = h; }
}
public struct CameraShakeEvent
{
    public float intensity;   // 震屏强度
}
public struct PlayerMeleeEvent
{
    public float damage;          // 造成的伤害
    public Vector3 hitPoint;      // 攻击命中点（可选，供特效参考）
}

