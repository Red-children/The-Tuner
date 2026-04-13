using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Boss状态机类，负责管理Boss的不同状态和行为，通过FSM来控制Boss的状态切换和行为逻辑，使得Boss能够根据玩家的行为做出相应的反应，增强了游戏的挑战性和互动性，同时通过状态机的设计模式实现了Boss行为的集中管理和扩展性，方便后续添加新的状态和行为逻辑。
/// </summary>
public class BossFSM : MonoBehaviour
{
    [SerializeReference]
    public IState currentState;

   private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    // 依赖注入：外部传入数据
    public BossRuntime Runtime { get; private set; }
    public BossData Data => Runtime?.Data;  // 使用新的安全访问器

    public BossController Controller { get; private set; }  // 新增

    //里氏替换 用工厂的接口确保能装下所有的工厂类型
    public void Initialize(BossRuntime runtime, BossController controller)
    {
        Runtime = runtime;
        Controller = controller;
        states = new Dictionary<StateType, IState>();
        states.Add(StateType.Idle, new BossIdleState(Controller));
        states.Add(StateType.Chase, new BossChaseState(Controller));
        states.Add(StateType.Attack, new BossAttackState(Controller));
        states.Add(StateType.Wound, new BossWoundState(Controller));  // 取消注释
        states.Add(StateType.Dead, new BossDeadState(Controller));    // 新增
        ChangeState(StateType.Idle);
    }

    public void ChangeState(StateType newState)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }
        currentState = states[newState];
        currentState.OnStart();
    }
    
   void Update() { currentState?.OnUpdate(); }
    

}
