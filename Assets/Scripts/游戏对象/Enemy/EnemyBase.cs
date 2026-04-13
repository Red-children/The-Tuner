using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人基类，所有敌人的父类
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("基础属性")]
    public Room ownerRoom; // 所属房间
    public Transform target;        
    
    [Header("组件")]
    public SpriteRenderer spriteRenderer;   //图片
    
    // 状态
    protected bool isDead = false;      
    protected bool isWounded = false;   
    

    
    // 被杀死时调用
    public abstract void OnKilled();
    
    // 死亡协程
    public abstract IEnumerator DeathCoroutine();
    
    // 受伤
   public abstract void Wound(float damage, RhythmRank rank);
    
    // 显示伤害飘字
    public abstract void ShowDamageText(Vector3 targetPosition, float damage, RhythmRank rank);
    
    // 抽象方法，子类实现具体行为
    protected abstract void UpdateBehavior();
    
    protected virtual void Awake()
    {
        // 设置标签和层级
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        
        // 获取组件
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    //包装一层Updata主要是为了实现死亡后停止组件的更新
    private void Update()
    {
        if (!isDead)
        {
            UpdateBehavior();
        }
    }

}
