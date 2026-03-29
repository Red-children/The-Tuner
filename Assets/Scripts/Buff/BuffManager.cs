using System.Collections.Generic;
using UnityEngine;

public class BuffInstance
{
    public BuffData data;
    public int currentStacks = 1;
    public float remainingTime; // 如果有持续时间

    public BuffInstance(BuffData data)
    {
        this.data = data;
        this.currentStacks = 1;
    }
}


public class BuffManager : MonoBehaviour
{
    private List<BuffInstance> buffs = new List<BuffInstance>();
    private PlayerStats stats;
    private PlayerWeapon weapon;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        weapon = GetComponent<PlayerWeapon>();
        if (stats == null) Debug.LogError("BuffManager: PlayerStats 组件未找到");
    }

    public void AddBuff(BuffData buffData)
    {
        var existing = buffs.Find(b => b.data == buffData);
        if (existing != null && buffData.isStackable)
        {
            if (existing.currentStacks < buffData.maxStack)
            {
                RemoveBuffEffect(existing);
                existing.currentStacks = Mathf.Min(existing.currentStacks + 1, buffData.maxStack);
                ApplyBuffEffect(existing);
                EventBus.Instance.Trigger(new BuffStackChangedEvent { buff = existing });
                Debug.Log($"[Buff] 叠加 {buffData.buffName}，当前层数：{existing.currentStacks}");
            }
            else
            {
                Debug.Log($"[Buff] {buffData.buffName} 已达最大层数");
            }
        }
        else if (existing == null)
        {
            var newBuff = new BuffInstance(buffData);
            buffs.Add(newBuff);
            ApplyBuffEffect(newBuff);
            EventBus.Instance.Trigger(new BuffAddedEvent { buff = newBuff });
            Debug.Log($"[Buff] 添加 {buffData.buffName}");
        }
    }

    public void RemoveBuff(BuffInstance buff)
    {
        RemoveBuffEffect(buff);
        buffs.Remove(buff);
        EventBus.Instance.Trigger(new BuffRemovedEvent { buff = buff });
    }

    private void ApplyBuffEffect(BuffInstance buff)
    {
        switch (buff.data.type)
        {
            case BuffType.IncreaseDamage:
                stats.ModifyAttack((int)buff.data.value * buff.currentStacks);
                break;
            case BuffType.IncreaseFireRate:
                break;
            case BuffType.IncreaseMoveSpeed:
                stats.ModifyMoveSpeed(buff.data.value * buff.currentStacks);
                break;
            case BuffType.IncreaseMaxHealth:
                // 增加最大生命值需要同时增加当前血量，由 PlayerStats 提供方法
                stats.ModifyMaxHealth((int)buff.data.value * buff.currentStacks);
                break;
                // 其他类型可类似扩展...
        }
    }

    private void RemoveBuffEffect(BuffInstance buff)
    {
        switch (buff.data.type)
        {
            case BuffType.IncreaseDamage:
                stats.ModifyAttack(-(int)buff.data.value * buff.currentStacks);
                break;
            case BuffType.IncreaseFireRate:
           
                break;
            case BuffType.IncreaseMoveSpeed:
                stats.ModifyMoveSpeed(-buff.data.value * buff.currentStacks);
                break;
            case BuffType.IncreaseMaxHealth:
                stats.ModifyMaxHealth(-(int)buff.data.value * buff.currentStacks);
                break;
        }
    }

    // 检查是否有某个Buff
    public bool HasBuff(BuffData buffData) => buffs.Exists(b => b.data == buffData);
    public List<BuffInstance> GetBuffs() => buffs;
}