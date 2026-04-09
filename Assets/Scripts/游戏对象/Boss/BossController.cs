using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class BossController : EnemyBase
{
    public BossData bossData;

    public BossFSM manager;
    public BossRuntime runtime;

    public void Awake()
    {


        manager = GetComponent<BossFSM>();
        runtime = GetComponent<BossRuntime>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(manager == null) Debug.LogError("BossFSM组件未找到！");
        if(runtime == null) Debug.LogError("BossRuntime组件未找到！");
        if(spriteRenderer == null) Debug.LogError("SpriteRenderer组件未找到！");

        manager.Initialize(runtime, this); // 传入运行时数据和控制器引用
        runtime.Init(bossData); // 传入控制器引用
    }

  
    public override void Wound(float damage)
    {
        if(isDead) return; // 已经死亡，避免重复调用
        runtime.currentHealth -= damage;
        if(runtime.currentHealth <= 0)
        {
            runtime.currentHealth = 0;
            OnKilled();
        }
        ShowDamageText(transform.position, damage);
    }

    public override void OnKilled()
    {
        if(isDead) return; // 已经死亡，避免重复调用
        isDead = true;

        if(ownerRoom != null)
        {
            ownerRoom.UnregisterEnemy(this);
        }
        else
        {
            Debug.LogWarning("BossController: ownerRoom未设置，无法注销敌人！");
        }
        // 触发敌人死亡事件（传递敌人信息）
        EventBus.Instance.Trigger(new EnemyDiedStruct(this, transform.position));
        
    }



    public override void ShowDamageText(Vector3 targetPosition, float damage)
    {
       // 使用安全的属性访问器
        if (runtime?.DamageTextPrefab == null) return;
        GameObject dmgObj = Instantiate(runtime.DamageTextPrefab, targetPosition, Quaternion.identity);
        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
        dmgNumber?.SetDamage(damage);
    }

    
    protected override IEnumerator DeathCoroutine()
    {
        // 标记为死亡状态
        isDead = true;
        
        // 调用OnKilled方法处理死亡逻辑
        OnKilled();
        
        
        yield return null;
        Destroy(this.gameObject);
    }

    protected override void UpdateBehavior()
    {
        //好像不太需要这个东西
    }


}