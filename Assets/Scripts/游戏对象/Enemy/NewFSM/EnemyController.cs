using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// 敌人控制器类，继承自EnemyBase，负责管理敌人的行为和状态，包括初始化数据、处理受伤和死亡逻辑、显示伤害飘字等，同时通过状态机（FSM）来控制敌人的不同状态和行为，使得敌人能够根据玩家的行为做出相应的反应，增强了游戏的互动性和挑战性。
/// </summary>
public class EnemyController : EnemyBase
{


    [Header("敌人数据")]
    public EnemyData data;

    [Header("敌人组件")]
    public WeaponInfo weapon;               
    public Collider2D chaseArea;            // 追逐范围碰撞体
    public Transform patrolCenter;          // 巡逻中心点
    public Transform[] patrolPoints;        // 巡逻点数组

    [Header("状态机组件")]
    [SerializeField] private EnemyRuntime runtime;
    [SerializeField] private FSM fsm;

    void Awake()
    {
        // 获取组件引用，如果未手动赋值则自动查找
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        //if (animator == null) animator = GetComponent<Animator>();
        if (weapon == null) weapon = GetComponentInChildren<WeaponInfo>();

        // 初始化运行时数据（仅在运行时创建）
        if (Application.isPlaying)
        {
            runtime = gameObject.AddComponent<EnemyRuntime>();
            runtime.Init(data);

            // 初始化 FSM 并注册状态
            fsm = gameObject.AddComponent<FSM>();
            IStateFactory factory = GetStateFactory(); // 根据敌人类型选择工厂
            fsm.Initialize(runtime, factory, this);
        }
    }
    // 实现基类的抽象方法
    protected override void UpdateBehavior()
    {
        // EnemyController使用状态机，这里不需要额外的行为逻辑
        // 状态机的更新在FSM类中处理
    }

    // 判断敌人类型并返回对应的状态工厂
    private IStateFactory GetStateFactory()
    {
        if (data is MeleeEnemyData) return new MeleeStateFactory();
        if (data is RangedEnemyData) return new RangedStateFactory();
        //if (data is ExplosiveEnemyData) return new ExplosiveStateFactory();
        return null;
    }

    // 受伤处理，包含受伤状态切换
    public override void Wound(float damage)
    {
        if (runtime.getHit) return; // 防止重复受伤
        runtime.getHit = true;
        runtime.currentHealth -= damage;
        
        // 使用运行时数据副本显示伤害
        ShowDamageText(this.transform.position, damage);

        // 无论是否致命伤害，都先切换到Wound状态
        // 这样可以确保EnemyHitEvent事件被触发，连击数正常增长
        fsm?.ChangeState(StateType.Wound);
    }

    // 死亡处理
    public void Dead()
    {
        StartCoroutine(DeadCoroutine());
    }

    // 死亡协程，延迟销毁以确保受伤状态有足够时间处理
    private IEnumerator DeadCoroutine()
    {
        runtime.getHit = false;
        
        // 调用基类的OnKilled方法
        OnKilled();
        
        // 等待1秒，确保受伤状态有足够时间处理
        yield return null;
        
        Destroy(gameObject);
    }

    // 显示伤害飘字
    public override void ShowDamageText(Vector3 TargatPosition, float damage)
    {
        // 使用安全的属性访问器
        if (runtime?.DamageTextPrefab == null) return;
        GameObject dmgObj = Instantiate(runtime.DamageTextPrefab, TargatPosition, Quaternion.identity);
        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
        dmgNumber?.SetDamage(damage);
    }



    // 被杀死时调用
    public override void OnKilled()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} 已被杀死");
        
        // 触发敌人死亡事件（传递敌人信息）
        EventBus.Instance.Trigger(new EnemyDiedStruct(this, transform.position));
        
        // 注销敌人（添加null检查避免异常）
        if (ownerRoom != null)
        {
            ownerRoom.UnregisterEnemy(this);
            Debug.Log($"{gameObject.name} 已从房间注销");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 的ownerRoom为null，无法注销");
        }

        //// 播放死亡特效（使用安全的属性访问器）
        //if (runtime?.DeadEff != null)
        //    Instantiate(runtime.DeadEff, transform.position, transform.rotation);
    }

    // 死亡协程
    protected override IEnumerator DeathCoroutine()
    {
        // 标记为死亡状态
        isDead = true;
        
        // 调用OnKilled方法处理死亡逻辑
        OnKilled();
        
        // 等待一帧，确保死亡逻辑执行完成
        yield return null;
        
        // 销毁游戏对象
        Destroy(gameObject);
    }

    // ��ײ��⣨�ӵ����У�
  
    public void OnPlayerEnter(Transform player)
    {
        if (runtime != null)
        {
            runtime.target = player;
            // ��ѡ�����ž����
        }
    }

    // ����˳�׷��Χ������
    public void OnPlayerExit(Transform player)
    {
        if (runtime != null && runtime.target == player)
        {
            runtime.target = null;
        }
    }

    public Vector2 GetAttackWorldPos()
    {
        //  
        if (data is MeleeEnemyData meleeData)
        {
            float dir = spriteRenderer.flipX ? -1f : 1f;
            return (Vector2)transform.position + new Vector2(dir * meleeData.attackOffset.x, meleeData.attackOffset.y);
        }
        // ������ǽ�ս���ˣ���������λ�ã����ף�
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
            
            Vector2 attackPos = Application.isPlaying ? GetAttackWorldPos() : (Vector2)transform.position + (Vector2)(meleeData.attackOffset * (spriteRenderer ? (spriteRenderer.flipX ? -1 : 1) : 1));
            Gizmos.DrawWireSphere(attackPos, meleeData.attackRange);
            // ��ѡ�����ƹ�����
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPos, 0.1f);
        }
        else if (data is RangedEnemyData rangedData)
        {
           
            Gizmos.DrawWireSphere(transform.position, rangedData.attackRange);
        }
        
    }
    


}