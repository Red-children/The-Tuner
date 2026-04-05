using System.Collections.Generic;
using UnityEngine;

//单个Buff实例
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
    private List<BuffInstance> buffs = new List<BuffInstance>(); //所有的buff
    private PlayerStats stats;                                   //玩家的数据   
    private PlayerWeapon weapon;                                 //玩家的武器   

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        weapon = GetComponent<PlayerWeapon>();
        if (stats == null) Debug.LogError("BuffManager: PlayerStats 组件未找到");
    }

    public void AddBuff(BuffData buffData)
    {
        var existing = buffs.Find(b => b.data == buffData); //寻找是否相同类型的bUff
        //有相同类型并且可以叠加
        if (existing != null && buffData.isStackable)
        {
            //当前层数小于最大层数 意味着可以正常叠层
            if (existing.currentStacks < buffData.maxStack)
            {
                //先移除再添加，这是由于计算逻辑导致的
                RemoveBuffEffect(existing);
                //计算当前层数
                existing.currentStacks = Mathf.Min(existing.currentStacks + 1, buffData.maxStack);
                //重新应用
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
            var newBuff = new BuffInstance(buffData);   //
            buffs.Add(newBuff);         
            ApplyBuffEffect(newBuff);   
            EventBus.Instance.Trigger(new BuffAddedEvent { buff = newBuff });
            Debug.Log($"[Buff] 添加 {buffData.buffName}");
        }
    }

    /// <summary>
    /// 移除Buff效果
    /// </summary>
    /// <param name="buff"></param>
    public void RemoveBuff(BuffInstance buff)
    {
        RemoveBuffEffect(buff);
        buffs.Remove(buff);
        EventBus.Instance.Trigger(new BuffRemovedEvent { buff = buff });
    }

    /// <summary>
    /// 应用buff效果
    /// </summary>
    /// <param name="buff"></param>
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