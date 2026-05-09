using System;
using System.Collections.Generic;

using UnityEngine;


#region 定义状态类型枚举
public enum StateType
{
    Idle,       // 空闲状态
    Patrol,     // 巡逻状态
    Chase,      // 追逐状态
    Attack,     // 攻击状态
    Wound,      // 受击状态
    Dead,       // 死亡状态
    Approach,    // 接近状态（新增）用于优化追逐行为，在玩家进入追逐范围但未进入攻击范围时切换到该状态，调整朝向并准备攻击
    NoiseScreamWarning,   // 噪声嘶吼预警
    NoiseScreamAttack,    // 噪声嘶吼释放
    NoiseStun,             // 噪声眩晕（被完美打断）
    Confused,   // 疑惑状态：丢失视野后短暂停留，尝试重新发现玩家
    BossRangedAttack, // Boss远程攻击
}
#endregion

public struct EnemyDiedStruct
{
    public EnemyBase enemy;  // 死亡的敌人引用
    public Vector3 deathPosition;  // 死亡位置

    public EnemyDiedStruct(EnemyBase enemy, Vector3 position)
    {
        this.enemy = enemy;
        this.deathPosition = position;
    }
}


//仅负责状态的切换
public class FSM : MonoBehaviour
{
    public Animator animator;  // 用于播放动画的组件引用
    [SerializeField]
    public IState currentState;
    [SerializeField] private string currentStateName; // 用于 Inspector 调试
    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    // 依赖注入：外部传入数据
    public EnemyRuntime Runtime { get; private set; }
    public EnemyData Data => Runtime?.Data;  // 使用新的安全访问器

    public EnemyController Controller { get; private set; }  // 新增

    //里氏替换 用工厂的接口确保能装下所有的工厂类型
    public void Initialize(EnemyRuntime runtime, IStateFactory factory, EnemyController controller)
    {
        animator = GetComponent<Animator>();
        Runtime = runtime;
        Controller = controller;  // 保存控制器引用


        // 通过工厂创建所有状态
        states.Add(StateType.Idle, factory.CreateIdleState(this));
        states.Add(StateType.Patrol, factory.CreatePatrolState(this));
        states.Add(StateType.Chase, factory.CreateChaseState(this));
        states.Add(StateType.Attack, factory.CreateAttackState(this));
        states.Add(StateType.Wound, factory.CreateWoundState(this));
        states.Add(StateType.Approach, factory.CreateApproachState(this));
        states.Add(StateType.Dead, factory.CreateDeadState(this));
        //states.Add(StateType.Confused, new EnemyConfusedState(this));

        // 根据数据类型注册特殊状态
        if (Data is NoiseMonsterData)
        {
            states.Add(StateType.NoiseScreamWarning, new NoiseScreamWarningState(this));
        
            states.Add(StateType.NoiseScreamAttack, new NoiseScreamAttackState(this));
            states.Add(StateType.NoiseStun, new NoiseStunState(this));
        }

        if (Data is BossData)
        {
            states[StateType.Chase] = new BossChaseState(this);
            states[StateType.Wound] = new BossWoundState(this);
            states[StateType.Dead] = new BossDeadState(this);
            states.Add(StateType.BossRangedAttack, new BossRangedAttackState(this));
        }

        ChangeState(StateType.Idle);
    }

    public void ChangeState(StateType newState)
    {
        currentState?.OnExit();
        currentState = states[newState];
        currentState.OnStart();
        Controller?.ResetStateTimer();
    }

    public void RegisterState(StateType type, IState state)
    {
        states[type] = state;
    }

    void Update()
    {
        currentState?.OnUpdate();
        Controller?.UpdateWeaponAim();

        // 更新调试显示
        if (currentState != null)
            currentStateName = currentState.GetType().Name;
    }

    
}