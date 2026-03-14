using System.Collections.Generic;
using UnityEngine;

public enum TrapType
{
    Spike,
    SlowFloor,
    SpeedFloor
}

[System.Serializable]
public class TrapStats
{
    public TrapType trapType;

    [Header("Spike")]
    public float damage = 10f;
    public float activeTime = 1f;
    public float inactiveTime = 2f;

    [Header("Buff")]
    public float buffDuration = 3f;
    public float speedMultiplier = 0.5f;
}

[CreateAssetMenu(fileName = "TrapBase", menuName = "Trap/TrapBase")]
public class TrapBase : ScriptableObject
{
    public List<TrapStats> trapList;

    public TrapStats GetTrapStats(TrapType type)
    {
        foreach (var trap in trapList)
        {
            if (trap.trapType == type)
                return trap;
        }

        Debug.LogWarning("TrapStats not found: " + type);
        return null;
    }
}