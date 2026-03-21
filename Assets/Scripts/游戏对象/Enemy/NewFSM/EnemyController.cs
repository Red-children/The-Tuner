using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyController : MonoBehaviour
{


    [Header("数据配置")]
    public EnemyData data;

    public Room ownerRoom;

    [Header("场景组件")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public WeaponInfo weapon;
    public Collider2D chaseArea;            // 追逐范围触发器
    public Transform patrolCenter;          // 巡逻中心点
    public Transform[] patrolPoints;        // 巡逻点数组




    public EnemyRuntime runtime;
    public FSM fsm;

    void Awake()
    {
        // 创建运行时组件
        runtime = gameObject.AddComponent<EnemyRuntime>();
        runtime.Init(data);

        // 获取场景组件（如果未手动赋值，自动查找）
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (weapon == null) weapon = GetComponentInChildren<WeaponInfo>();

        // 创建 FSM 并注入数据
        fsm = gameObject.AddComponent<FSM>();
        IStateFactory factory = GetStateFactory(); // 根据敌人类型选择工厂
        fsm.Initialize(runtime, factory,this);
    }

    //判断该用什么类型的工厂
    private IStateFactory GetStateFactory()
    {
        if (data is MeleeEnemyData) return new MeleeStateFactory();
        if (data is RangedEnemyData) return new RangedStateFactory();
        //if (data is ExplosiveEnemyData) return new ExplosiveStateFactory();
        return null;
    }

    // 受伤方法（由子弹或状态机调用）
    public void Wound(float damage)
    {
        if (runtime.getHit) return; // 防止连续触发
        runtime.getHit = true;
        runtime.currentHealth -= damage;
        ShowDamageText(this.transform.position ,damage);

        if (runtime.currentHealth <= 0)
        {
            Dead();
        }
        else
        {
            // 切换到受击状态（通过 FSM）
            fsm?.ChangeState(StateType.Wound);
        }
    }

    // 死亡方法
    public void Dead()
    {
        runtime.getHit = false;
        EventBus.Instance.Trigger(new EnemyDiedStruct());
        if (data.deadEff != null)
            Instantiate(data.deadEff, transform.position, transform.rotation);
        Destroy(gameObject);
        ownerRoom?.UnregisterEnemy(this);
    }

    // 显示伤害飘字
    public void ShowDamageText(Vector3 TargatPosition , float damage )
    {
        if (data.damageTextPrefab == null) return;
        GameObject dmgObj = Instantiate(data.damageTextPrefab, TargatPosition, Quaternion.identity);
        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
        dmgNumber?.SetDamage(damage);
    }

    // 碰撞检测（子弹击中）
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.DestroyMyself();
                Wound(bullet.damage);
            }
        }
    }
    public void OnPlayerEnter(Transform player)
    {
        if (runtime != null)
        {
            runtime.target = player;
            // 可选：播放警觉音效等
        }
    }

    // 玩家退出追逐范围触发器
    public void OnPlayerExit(Transform player)
    {
        if (runtime != null && runtime.target == player)
        {
            runtime.target = null;
        }
    }

    public Vector2 GetAttackWorldPos()
    {
        // 确保是近战敌人且数据存在
        if (data is MeleeEnemyData meleeData)
        {
            float dir = spriteRenderer.flipX ? -1f : 1f;
            return (Vector2)transform.position + new Vector2(dir * meleeData.attackOffset.x, meleeData.attackOffset.y);
        }
        // 如果不是近战敌人，返回自身位置（保底）
        return transform.position;
    }

    public void UpdateWeaponAim()
    {
        if (weapon != null && runtime.target != null)
        {
            Vector2 direction = runtime.target.position - weapon.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            weapon.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.red;
        if (data is MeleeEnemyData meleeData)
        {
            // 绘制近战攻击范围（以攻击点为圆心）
            Vector2 attackPos = Application.isPlaying ? GetAttackWorldPos() : (Vector2)transform.position + (Vector2)(meleeData.attackOffset * (spriteRenderer ? (spriteRenderer.flipX ? -1 : 1) : 1));
            Gizmos.DrawWireSphere(attackPos, meleeData.attackRange);
            // 可选：绘制攻击点
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPos, 0.1f);
        }
        else if (data is RangedEnemyData rangedData)
        {
            // 绘制远程攻击范围（以自身为中心）
            Gizmos.DrawWireSphere(transform.position, rangedData.attackRange);
        }
        // 其他类型可继续扩展
    }
    


}