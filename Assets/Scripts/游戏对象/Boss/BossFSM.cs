using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Boss状态机类，负责管理Boss的不同状态和行为，通过FSM来控制Boss的状态切换和行为逻辑，使得Boss能够根据玩家的行为做出相应的反应，增强了游戏的挑战性和互动性，同时通过状态机的设计模式实现了Boss行为的集中管理和扩展性，方便后续添加新的状态和行为逻辑。
/// </summary>
public class BossFSM : MonoBehaviour
{
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
        Controller = controller;  // 保存控制器引用
        states.Add(StateType.Idle, new BossIdleState(Controller));  // 传入控制器引用
        states.Add(StateType.Attack, new BossAttackState(Controller));    
        //继续加入想要的状态
        ChangeState(StateType.Idle);  // 初始为Idle状态
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
