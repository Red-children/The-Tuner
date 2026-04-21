using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EnemyConfusedState : EnemyStateBase
{
    private float timer;
    private float confusedDuration = 2.5f; // 疑惑持续时间，可在数据中配置
    private Vector2? lastKnownPlayerPosition; // 最后看到玩家的位置

    public EnemyConfusedState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        Debug.Log($"[{controller.name}] 进入疑惑状态");
        manager.animator.SetTrigger("Idle"); // 或者播放疑惑动画
        timer = 0f;
        
        // 记录最后看到玩家的位置（用于可能的调查移动，可选）
        if (runtime.target != null)
            lastKnownPlayerPosition = runtime.target.position;
        else
            lastKnownPlayerPosition = null;

        // 停止移动
        NavMeshAgent agent = controller.agent;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public override void OnUpdate()
    {
        timer += Time.deltaTime;

        // 如果在疑惑期间重新看到玩家，立即恢复追击
        if (controller.CanSeePlayer())
        {
            Debug.Log($"[{controller.name}] 疑惑中重新发现玩家，恢复追击");
            manager.ChangeState(StateType.Chase);
            return;
        }

        // 超时，彻底放弃，返回巡逻
        if (timer >= confusedDuration)
        {
            Debug.Log($"[{controller.name}] 疑惑超时，放弃追击，返回巡逻");
            runtime.target = null;
            runtime.isPursuing = false;
            runtime.ignoreTargetUntilTime = Time.time + 3f; // 冷却
            manager.ChangeState(StateType.Patrol);
        }
    }

    public override void OnExit()
    {
        NavMeshAgent agent = controller.agent;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        lastKnownPlayerPosition = null;
    }
}