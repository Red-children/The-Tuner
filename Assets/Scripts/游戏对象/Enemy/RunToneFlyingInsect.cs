using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 跑调飞虫 - 基础敌人
/// 行为：成群结队同步BPM向玩家冲撞
/// </summary>
public class RunToneFlyingInsect : EnemyBase
{
    // 数据管理器
    private RunToneFlyingInsectDataManager dataManager;
    
    // 模块
    private RunToneFlyingPerceptionModule perceptionModule;
    private RunToneFlyingMovementModule movementModule;
    private RunToneFlyingHealthModule healthModule;
    private RunToneFlyingAttackModule attackModule;
    private RunToneFlyingEffectModule effectModule;
    
    protected override void UpdateBehavior()
    {
        // 更新各个模块
        perceptionModule.Update();
        movementModule.Update();
        attackModule.Update();
    }
    
    protected override void Awake()
    {
        // 调用基类的Awake方法
        base.Awake();
        
        // 确保RunToneFlyingInsectDataManager组件存在
        dataManager = GetComponent<RunToneFlyingInsectDataManager>();
        if (dataManager == null)
        {
            dataManager = gameObject.AddComponent<RunToneFlyingInsectDataManager>();
        }
        
        // 初始化模块
        perceptionModule = new RunToneFlyingPerceptionModule(this);
        movementModule = new RunToneFlyingMovementModule(this, perceptionModule);
        healthModule = new RunToneFlyingHealthModule(this);
        attackModule = new RunToneFlyingAttackModule(this, perceptionModule);
        effectModule = new RunToneFlyingEffectModule(this);
        
        // 设置模块间的引用
        movementModule.SetAttackModule(attackModule);
    }
    


    // 实现死亡方法
    public override void OnKilled()
    {
        isDead = true;      //修改死亡标签
        ownerRoom?.UnregisterEnemy(this);//从房间里注销敌人 确保清完所有怪物房间门可以正常打开
        
        // 播放死亡特效
        effectModule.PlayDeathEffect();
        
        // 触发敌人死亡事件
        EventBus.Instance.Trigger(new EnemyDiedStruct());
        
        StartCoroutine(DeathCoroutine());
        // 飞虫特有的死亡逻辑
        Debug.Log("RunToneFlyingInsect killed");

    }
    

    // 实现死亡协程（事件系统的问题死亡后不能直接摧毁敌人，需要等待UI更新）
    public override IEnumerator DeathCoroutine()
    {
        // 飞虫特有的死亡动画或效果
        Debug.Log("RunToneFlyingInsect death coroutine");
        
        // 等待动画完成或直接销毁
        yield return null;
        Destroy(gameObject);
    }
    

    // 实现受伤方法
    public override void Wound(float damage)
    {
        if (isDead) return;
        
        isWounded = true;

        // 使用血量控制模块处理伤害
        healthModule.TakeDamage(damage);
        
        // 播放受伤特效
        effectModule.PlayWoundEffect();
        
        // 飞虫特有的受伤逻辑
        Debug.Log("RunToneFlyingInsect wounded: " + damage);
    }
    
    // 实现显示伤害飘字方法
    public override void ShowDamageText(Vector3 targetPosition, float damage)
    {
        // 使用效果模块显示伤害飘字
        effectModule.ShowDamageText(targetPosition, damage);
        
        // 飞虫特有的伤害飘字效果
        Debug.Log("RunToneFlyingInsect show damage text: " + damage);
    }
}
