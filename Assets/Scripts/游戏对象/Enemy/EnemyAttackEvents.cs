using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackEvents : MonoBehaviour
{
    private FSM fsm;

    private void Start()
    {
        fsm = GetComponent<FSM>();
    }

    // 被动画事件调用
    public void OnAttackHit()
    {
        // 获取当前状态，如果是 AttackState，则调用它的 OnAttackHit 方法
        if (fsm.currentState is EnemyMeleeAttackState attackState)
        {
            attackState.OnAttackHit();
        }
    }
}
