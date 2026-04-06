using System;
using System.Collections.Generic;

using UnityEngine;


#region 敌人枚举类型
public enum EnemyType
{
    Melee,   // 近战
    Ranged   // 远程
}
#endregion

#region 定义状态类型枚举
public enum StateType
{
    Idle,       // 空闲状态
    Patrol,     // 巡逻状态
    Chase,      // 追逐状态
    Attack,     // 攻击状态
    Wound,      // 受击状态
    Dead,       // 死亡状态
    Approach    // 接近状态（新增）用于优化追逐行为，在玩家进入追逐范围但未进入攻击范围时切换到该状态，调整朝向并准备攻击

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


#region 敌人的参数
[Serializable]
public class Parameter
{
    [Header("★ 基础信息")]
    public EnemyType enemyType;          // 用于在 FSM 中做类型判断

    [Header("🎮 数据配置")]
    public EnemyData data;               // 运行时引用对应的 ScriptableObject

    // 运行时状态（由代码自动管理）
    public Transform target;
    public bool getHit;

    [Header("🎮 追击范围")]
    public Collider2D ChaseArea;
}
#endregion

//仅负责状态的切换
public class FSM : MonoBehaviour
{
    public IState currentState;
    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    // 依赖注入：外部传入数据
    public EnemyRuntime Runtime { get; private set; }
    public EnemyData Data => Runtime?.Data;  // 使用新的安全访问器

    public EnemyController Controller { get; private set; }  // 新增

    //里氏替换 用工厂的接口确保能装下所有的工厂类型
    public void Initialize(EnemyRuntime runtime, IStateFactory factory, EnemyController controller)
    {
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


        ChangeState(StateType.Idle);
    }

    public void ChangeState(StateType newState)
    {
        currentState?.OnExit();
        currentState = states[newState];
        currentState.OnStart();
    }

    void Update()
    {
        currentState?.OnUpdate();
        Controller?.UpdateWeaponAim();  // 确保武器实时指向目标
    }
}