// 事件结构体（放在单独的文件或这里）
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

