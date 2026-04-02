using UnityEngine;

/// <summary>
/// 血量控制模块 - 处理敌人的血量和死亡逻辑
/// </summary>
public class RunToneFlyingHealthModule
{
    private RunToneFlyingInsect owner;
    private RunToneFlyingInsectDataManager dataManager;
    
    /// <summary>
    /// 当前血量
    /// </summary>
    public float CurrentHealth { get { return dataManager.CurrentHealth; } }
    
    /// <summary>
    /// 最大血量
    /// </summary>
    public float MaxHealth { get { return dataManager.MaxHealth; } }
    
    /// <summary>
    /// 是否死亡
    /// </summary>
    public bool IsDead { get { return dataManager.CurrentHealth <= 0; } }
    
    /// <summary>
    /// 初始化血量控制模块
    /// </summary>
    /// <param name="owner">拥有者</param>
    public RunToneFlyingHealthModule(RunToneFlyingInsect owner)
    {
        this.owner = owner;
        this.dataManager = owner.GetComponent<RunToneFlyingInsectDataManager>();
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        
        dataManager.ModifyHealth(-damage);
        
        // 显示伤害飘字
        owner.ShowDamageText(owner.transform.position, damage);
        
        if (dataManager.CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    private void Die()
    {
        // 触发死亡事件
        owner.OnKilled();
    }
    
    /// <summary>
    /// 重置血量
    /// </summary>
    public void ResetHealth()
    {
        dataManager.SetMaxHealth(dataManager.MaxHealth);
    }
    
    /// <summary>
    /// 设置最大血量
    /// </summary>
    /// <param name="health">最大血量</param>
    public void SetMaxHealth(float health)
    {
        dataManager.SetMaxHealth(health);
    }
}
