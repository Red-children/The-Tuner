using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    //buff列表
    private List<BuffInstance> buffs = new List<BuffInstance>();
    public PlayerIObject player;       // 在Inspector中拖拽或自动获取

    private void Awake()
    {
        if (player == null) player = GetComponent<PlayerIObject>();
    }

    #region 添加Buff方法
    // 添加Buff
    public void AddBuff(BuffData buffData)
    {
        // 处理叠加
        var existing = buffs.Find(b => b.data == buffData);
        if (existing != null && buffData.isStackable)
        {
            if (existing.currentStacks < buffData.maxStack)
            {
                // 先移除旧效果（按当前层数）
                RemoveBuffEffect(existing);
                // 增加层数
                existing.currentStacks = Mathf.Min(existing.currentStacks + 1, buffData.maxStack);
                // 再应用新效果
                ApplyBuffEffect(existing);
                EventBus.Instance.Trigger(new BuffStackChangedEvent { buff = existing });
                Debug.Log($"[Buff] 叠加 {buffData.buffName}，当前层数：{existing.currentStacks}");
            }
            else
            {
                Debug.Log($"[Buff] {buffData.buffName} 已达最大层数，无法继续叠加");
            }
        }
        else if (existing == null)
        {
            var newBuff = new BuffInstance(buffData);
            buffs.Add(newBuff);
            ApplyBuffEffect(newBuff);
            EventBus.Instance.Trigger(new BuffAddedEvent { buff = newBuff });
            Debug.Log($"[Buff] 添加 {buffData.buffName}，当前层数：1");
        }
    }
    #endregion

    #region 移除Buff

    // 移除Buff
    public void RemoveBuff(BuffInstance buff)
    {
        RemoveBuffEffect(buff);
        buffs.Remove(buff);
        EventBus.Instance.Trigger(new BuffRemovedEvent { buff = buff });
    }
    #endregion

    #region 应用Buff
    // 应用效果（数值型直接修改玩家属性，机制型通过事件或挂载脚本）
    private void ApplyBuffEffect(BuffInstance buff)
    {
        switch (buff.data.type)
        {
            case BuffType.IncreaseDamage:
                player.atk += (int)buff.data.value * buff.currentStacks;
                Debug.Log($"[Buff] 攻击力增加 {buff.data.value * buff.currentStacks}，当前攻击力：{player.atk}");
                EventBus.Instance.Trigger(new PlayerAtkChange {atk =this.player.atk });
                break;
            case BuffType.IncreaseFireRate:
                player.currentWeapon.stats.fireRate *= (1 + buff.data.value * buff.currentStacks);
                Debug.Log($"[Buff] 攻速增加 {buff.data.value * buff.currentStacks}，当前攻击力：{player.atk}");
                break;
            //case BuffType.SpecialEffect:
            //    // 生成特效或挂载逻辑脚本
            //    if (buff.data.effectPrefab != null)
            //    {
            //        GameObject effect = Instantiate(buff.data.effectPrefab, player.transform);
            //        // 让特效脚本自己处理逻辑，它可以通过EventBus订阅事件
            //    }
            //    break;
        }
    }
    #endregion


    private void RemoveBuffEffect(BuffInstance buff)
    {
        switch (buff.data.type)
        {
            case BuffType.IncreaseDamage:
                player.atk -= (int)buff.data.value * buff.currentStacks;
                break;
            case BuffType.IncreaseFireRate:
                player.currentWeapon.stats.fireRate /= (1 + buff.data.value * buff.currentStacks);
                break;
            //case BuffType.SpecialEffect:
            //    // 销毁挂载的特效或脚本
            //    // 可以通过事件通知特效脚本自我销毁
                //break;
        }
    }

    // 检查是否有某个Buff
    public bool HasBuff(BuffData buffData) => buffs.Exists(b => b.data == buffData);
    // 获取Buff实例列表（供UI使用）
    public List<BuffInstance> GetBuffs() => buffs;
}

// 运行时Buff实例
[System.Serializable]
public class BuffInstance
{
    public BuffData data;
    public int currentStacks = 1;
    public float remainingTime; // 如果有持续时间的话

    // 构造函数：从 BuffData 创建一个新实例
    public BuffInstance(BuffData data)
    {
        this.data = data;
        this.currentStacks = 1;
    }
}
