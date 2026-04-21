using UnityEngine;
using System;
using System.Collections.Generic;


[Serializable]
public class EnemyMeleeAttackState : EnemyStateBase
{
    private List<Collider2D> comboColliders;
    private int currentHitIndex;
    private bool attackFinished;

    public EnemyMeleeAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        // 从控制器获取配置好的碰撞体列表
        comboColliders = controller.comboHitColliders;
        currentHitIndex = 0;
        attackFinished = false;

        // 初始关闭所有碰撞体
        if (comboColliders != null)
        {
            foreach (var col in comboColliders)
                if (col) col.enabled = false;
        }

        controller.ShowAttackWarning();
        manager.animator.SetTrigger("Attack");
        Debug.Log($"[{controller.name}] 进入攻击状态，连击段数: {(comboColliders?.Count ?? 1)}");
    }

    public override void OnUpdate()
    {
        if (runtime.getHit)
        {
            manager.ChangeState(StateType.Wound);
        }
    }

    // 由动画事件（经Controller转发）调用，激活下一击碰撞体
    public void OnComboHit()
    {
        
        if (currentHitIndex >= comboColliders.Count) return;

        // 关闭所有碰撞体
        foreach (var col in comboColliders)
            if (col) col.enabled = false;

        // 激活当前索引的碰撞体
        var activeCollider = comboColliders[currentHitIndex];
        if (activeCollider != null)
        {
            activeCollider.enabled = true;
            var hitScript = activeCollider.GetComponent<EnemyWeaponHit>();
            if (hitScript) hitScript.ResetHitFlag();
        }

        currentHitIndex++;
        Debug.Log($"[{controller.name}] 连击第 {currentHitIndex} 段命中判定");
    }

    // 由动画事件调用，结束攻击状态
    public void OnAttackFinished()
    {
        if (attackFinished) return;
        attackFinished = true;
        Debug.Log($"[{controller.name}] 攻击结束，返回追逐");
        manager.ChangeState(StateType.Chase);
    }

    public override void OnExit()
    {
        // 关闭所有碰撞体
        if (comboColliders != null)
        {
            foreach (var col in comboColliders)
                if (col) col.enabled = false;
        }
        manager.animator.ResetTrigger("Attack");
    }
}