using UnityEngine;

/// <summary>
/// 效果模块 - 处理敌人的视觉和音效效果
/// </summary>
public class EffectModule
{
    private RunToneFlyingInsect owner;
    private RunToneFlyingInsectDataManager dataManager;
    
    /// <summary>
    /// 初始化效果模块
    /// </summary>
    /// <param name="owner">拥有者</param>
    public EffectModule(RunToneFlyingInsect owner)
    {
        this.owner = owner;
        this.dataManager = owner.GetComponent<RunToneFlyingInsectDataManager>();
    }
    
    /// <summary>
    /// 显示伤害飘字
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="damage">伤害值</param>
    public void ShowDamageText(Vector3 position, float damage)
    {
        if (dataManager.DamageTextPrefab != null)
        {
            GameObject dmgObj = Object.Instantiate(dataManager.DamageTextPrefab, position, Quaternion.identity);
            DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
            if (dmgNumber != null)
            {
                dmgNumber.SetDamage(damage);
            }
        }
    }
    
    /// <summary>
    /// 播放死亡特效
    /// </summary>
    public void PlayDeathEffect()
    {
        if (dataManager.DeathEffectPrefab != null)
        {
            Object.Instantiate(dataManager.DeathEffectPrefab, owner.transform.position, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// 播放受伤特效
    /// </summary>
    public void PlayWoundEffect()
    {
        // 这里可以添加受伤特效，例如粒子效果、音效等
        Debug.Log("RunToneFlyingInsect wounded effect");
    }
}
