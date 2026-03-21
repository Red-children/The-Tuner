using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;


public enum E_PlayerState
{
    Idle,
    Walk,
    Run,
    Jump,
    Attack,
    Hurt,
    Die
}

[Serializable]
public class PlayerParameter
{  
    public int atk; // 攻击力
    public bool getHit; // 是否受到攻击
    public float idleTime; // 等待时间
    public GameObject target; // 目标对象（例如玩家）
    public float moveSpeed; // 移动速度

    public WeaponInfo currentWeapon;   // 引用当前武器（可以从 PlayerIObject 获取）

    [Header("近战攻击设置")]
    public float meleeRange = 1.5f;          // 近战范围
    public LayerMask enemyLayer;              // 敌人层级
    public float meleeBaseDamage = 20f;       // 基础伤害
    public float meleeCooldown = 0.5f;        // 近战冷却
    public float lastMeleeTime = -999f;
    public float rhythmMultiplier = 1f; // 默认倍率1

    [Header("闪避设置")]
    public float dashDistance = 3f;          // 最大闪避距离
    public float dashDuration = 0.3f;        // 闪避持续时间

    public bool isDashing = false;             // 是否正在闪避
    public AnimationCurve dashCurve;              // 闪避位移曲线（可选，用于控制闪避的加速/减速效果）

    public float maxDashEnergy = 2;          // 闪避条上限
    public float currentDashEnergy = 2;   // 当前闪避条
    public float dashEnergyRegenRate = 1f;    // 闪避条恢复速率（每秒恢复多少）
    public bool isDashOnWindow = false;             // 是否在节奏窗口内可以闪避
}




//玩家状态机 
//打算完全重构玩家类 
public class PlayerFSM : MonoBehaviour
{
    public SpriteRenderer playerSpriteRenderer;
    public PlayerParameter parameter = new PlayerParameter();

    public IState currentState;
    public Dictionary<E_PlayerState, IState> states = new Dictionary<E_PlayerState, IState>();

    void Start()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        states.Add(E_PlayerState.Idle, new PlayerIdleState(this));
        states.Add(E_PlayerState.Walk, new PlayerWalkState(this));

        ChangeState(E_PlayerState.Idle); // 直接使用枚举切换
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
    }

    private void Update()
    {
        currentState?.OnUpdate();

        // 开火检测（全局）
        if (Input.GetMouseButton(0) && parameter.currentWeapon != null)
        {
            parameter.currentWeapon.Shoot();
        }

        // 近战攻击
        if (Input.GetKeyDown(KeyCode.V) && Time.time > parameter.lastMeleeTime + parameter.meleeCooldown)
        {
            MeleeAttack();
        }

        // 武器切换（数字键）
        if (Input.GetKeyDown(KeyCode.Alpha1))
            parameter.currentWeapon?.SwitchWeapon(WeaponType.Pistol);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            parameter.currentWeapon?.SwitchWeapon(WeaponType.Shotgun);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            parameter.currentWeapon?.SwitchWeapon(WeaponType.Rifle);

        // 武器切换（滚轮）
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && parameter.currentWeapon != null)
        {
            var weaponList = parameter.currentWeapon.weaponBase.weaponList;
            int count = weaponList.Count;
            if (count == 0) return;

            int currentIndex = weaponList.FindIndex(w => w.weaponType == parameter.currentWeapon.weaponType);
            if (currentIndex == -1)
            {
                Debug.LogWarning("当前武器类型不在武器列表中，默认切换到第一个");
                currentIndex = 0;
            }

            int delta = scroll > 0 ? 1 : -1;
            int newIndex = (currentIndex + delta + count) % count;
            WeaponType newType = weaponList[newIndex].weaponType;
            parameter.currentWeapon.SwitchWeapon(newType);
        }
    }

    public void ChangeState(E_PlayerState newState)
    {
        currentState?.OnExit();
        currentState = states[newState];
        currentState.OnStart();
    }

    private void MeleeAttack()
    {
        parameter.lastMeleeTime = Time.time;

        float finalDamage = (parameter.atk + parameter.meleeBaseDamage) * parameter.rhythmMultiplier;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, parameter.meleeRange, LayerMask.GetMask("Enemy"));
        foreach (var enemy in hitEnemies)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.Wound(finalDamage);
            }
        }

        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = finalDamage * 0.1f });
        EventBus.Instance.Trigger(new PlayerMeleeEvent { damage = finalDamage, hitPoint = transform.position });
    }

    private void OnRhythmData(RhythmData data)
    {
        parameter.rhythmMultiplier = (float)data.multiplier;
        parameter.isDashOnWindow = data.isInWindow;
    }
}