//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;



//public class EnemyController : EnemyBase
//{


//    [Header("��������")]
//    public EnemyData data;



//    [Header("�������")]

//    public WeaponInfo weapon;
//    public Collider2D chaseArea;            // ׷��Χ������
//    public Transform patrolCenter;          // Ѳ�����ĵ�
//    public Transform[] patrolPoints;        // Ѳ�ߵ�����




//    public EnemyRuntime runtime;
//    public FSM fsm;

//    void Awake()
//    {
//        // ��ȡ������������δ�ֶ���ֵ���Զ����ң�
//        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
//        // if (animator == null) animator = GetComponent<Animator>();
//        if (weapon == null) weapon = GetComponentInChildren<WeaponInfo>();

//        // ��������ʱ���
//        runtime = gameObject.AddComponent<EnemyRuntime>();
//        runtime.Init(data);

//        // ���� FSM ��ע������
//        fsm = gameObject.AddComponent<FSM>();
//        IStateFactory factory = GetStateFactory(); // ���ݵ�������ѡ�񹤳�
//        fsm.Initialize(runtime, factory,this);
//    }
    
//    private void Start()
//    {
//        // 查找并注册到房间
//        Room room = GetComponentInParent<Room>();
//        if (room != null)
//        {
//            RegisterToRoom(room);
//        }
//    }
    
//    // 实现基类的抽象方法
//    protected override void UpdateBehavior()
//    {
//        // EnemyController使用状态机，这里不需要额外的行为逻辑
//        // 状态机的更新在FSM类中处理
//    }

//    //�жϸ���ʲô���͵Ĺ���
//    private IStateFactory GetStateFactory()
//    {
//        if (data is MeleeEnemyData) return new MeleeStateFactory();
//        if (data is RangedEnemyData) return new RangedStateFactory();
//        //if (data is ExplosiveEnemyData) return new ExplosiveStateFactory();
//        return null;
//    }

//    // ���˷��������ӵ���״̬�����ã�
//    public void Wound(float damage)
//    {
//        if (runtime.getHit) return; // ��ֹ��������
//        runtime.getHit = true;
//        runtime.currentHealth -= damage;
//        ShowDamageText(this.transform.position ,damage);

//        // 无论是否致命伤害，都先切换到Wound状态
//        // 这样可以确保EnemyHitEvent事件被触发，连击数正常增长
//        fsm?.ChangeState(StateType.Wound);
        
//        // 调用基类的Wound方法
//        base.Wound(damage);
//    }

//    // 死亡处理
//    public void Dead()
//    {
//        StartCoroutine(DeadCoroutine());
//    }

//    // 死亡协程，延迟销毁以确保受伤状态有足够时间处理
//    private IEnumerator DeadCoroutine()
//    {
//        runtime.getHit = false;
//        EventBus.Instance.Trigger(new EnemyDiedStruct());
//        if (data.deadEff != null)
//            Instantiate(data.deadEff, transform.position, transform.rotation);
        
//        // 调用基类的OnKilled方法
//        OnKilled();
        
//        // 等待1秒，确保受伤状态有足够时间处理
//        yield return null;
        
//        Destroy(gameObject);
//    }

//    // ��ʾ�˺�Ʈ��
//    public void ShowDamageText(Vector3 TargatPosition , float damage )
//    {
//        if (data.damageTextPrefab == null) return;
//        GameObject dmgObj = Instantiate(data.damageTextPrefab, TargatPosition, Quaternion.identity);
//        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
//        dmgNumber?.SetDamage(damage);
//    }

//    // ��ײ��⣨�ӵ����У�
//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Bullet"))
//        {
//            Bullet bullet = other.GetComponent<Bullet>();
//            if (bullet != null)
//            {
//                bullet.DestroyMyself();
//                Wound(bullet.damage);
//            }
//        }
//    }
//    public void OnPlayerEnter(Transform player)
//    {
//        if (runtime != null)
//        {
//            runtime.target = player;
//            // ��ѡ�����ž�����Ч��
//        }
//    }

//    // ����˳�׷��Χ������
//    public void OnPlayerExit(Transform player)
//    {
//        if (runtime != null && runtime.target == player)
//        {
//            runtime.target = null;
//        }
//    }

//    public Vector2 GetAttackWorldPos()
//    {
//        // ȷ���ǽ�ս���������ݴ���
//        if (data is MeleeEnemyData meleeData)
//        {
//            float dir = spriteRenderer.flipX ? -1f : 1f;
//            return (Vector2)transform.position + new Vector2(dir * meleeData.attackOffset.x, meleeData.attackOffset.y);
//        }
//        // ������ǽ�ս���ˣ���������λ�ã����ף�
//        return transform.position;
//    }

//    public void UpdateWeaponAim()
//    {
//        if (weapon != null && runtime.target != null)
//        {
//            Vector2 direction = runtime.target.position - weapon.transform.position;
//            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//            weapon.transform.rotation = Quaternion.Euler(0, 0, angle);
//        }
//    }

//    private void OnDrawGizmosSelected()
//    {
//        if (data == null) return;

//        Gizmos.color = Color.red;
//        if (data is MeleeEnemyData meleeData)
//        {
//            // ���ƽ�ս������Χ���Թ�����ΪԲ�ģ�
//            Vector2 attackPos = Application.isPlaying ? GetAttackWorldPos() : (Vector2)transform.position + (Vector2)(meleeData.attackOffset * (spriteRenderer ? (spriteRenderer.flipX ? -1 : 1) : 1));
//            Gizmos.DrawWireSphere(attackPos, meleeData.attackRange);
//            // ��ѡ�����ƹ�����
//            Gizmos.color = Color.yellow;
//            Gizmos.DrawWireSphere(attackPos, 0.1f);
//        }
//        else if (data is RangedEnemyData rangedData)
//        {
//            // ����Զ�̹�����Χ��������Ϊ���ģ�
//            Gizmos.DrawWireSphere(transform.position, rangedData.attackRange);
//        }
//        // �������Ϳɼ�����չ
//    }
    


//}