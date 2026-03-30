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
    private PerceptionModule perceptionModule;
    private MovementModule movementModule;
    private HealthModule healthModule;
    private AttackModule attackModule;
    private EffectModule effectModule;
    
    protected override void UpdateBehavior()
    {
        // 更新各个模块
        perceptionModule.Update();
        movementModule.Update();
        attackModule.Update();
    }
    
    private void Awake()
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
        perceptionModule = new PerceptionModule(this);
        movementModule = new MovementModule(this, perceptionModule);
        healthModule = new HealthModule(this);
        attackModule = new AttackModule(this, perceptionModule);
        effectModule = new EffectModule(this);
    }
    
    // 实现注册到房间方法
    public override void RegisterToRoom(Room room)
    {
        ownerRoom = room;
        room.RegisterEnemy(this);
        // 飞虫特有的注册逻辑
        Debug.Log("RunToneFlyingInsect registered to room: " + room.name);
    }
    
    // 实现死亡方法
    public override void OnKilled()
    {
        isDead = true;
        ownerRoom?.UnregisterEnemy(this);
        
        // 播放死亡特效
        effectModule.PlayDeathEffect();
        
        // 触发敌人死亡事件
        EventBus.Instance.Trigger(new EnemyDiedStruct());
        
        StartCoroutine(DeathCoroutine());
        // 飞虫特有的死亡逻辑
        Debug.Log("RunToneFlyingInsect killed");
    }
    
    // 实现死亡协程
    protected override IEnumerator DeathCoroutine()
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
        
        // 触发敌人被命中的事件
        EventBus.Instance.Trigger(new EnemyHitEvent (1,RhythmManager.Instance.GetRank().rank));
        
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
