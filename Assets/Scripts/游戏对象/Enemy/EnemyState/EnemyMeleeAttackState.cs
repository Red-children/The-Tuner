using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAttackState : EnemyStateBase
{
    private float attackTimer;

    public EnemyMeleeAttackState(FSM manager):base(manager)
    {
    }

    public override void OnStart()
    {
        Debug.Log("进入近战攻击状态");
        attackTimer = 0f;
        parameter.animator.SetTrigger("Attack");
    }

    public override void OnUpdate()
    {
        if (parameter.getHit) { manager.ChangeState(StateType.Wound); return; }
        if (parameter.target == null || !IsTargetInRange())
        {
            manager.ChangeState(StateType.Chase);
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f) // 攻击间隔，可从配置读取
        {
            attackTimer = 0f;
            // 实际伤害在动画事件中触发，这里只做冷却
        }
    }

    public override void OnExit() { }

    private bool IsTargetInRange()
    {
        if (parameter.target == null) return false;
        Vector2 attackWorldPos = manager.GetAttackWorldPos();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackWorldPos, parameter.attackRange, parameter.targetLayer);
        foreach (var hit in hits)
            if (hit.transform == parameter.target) return true;
        return false;
    }

    // 动画事件调用
    public void OnAttackHit()
    {
        if (!IsTargetInRange()) return;
        PlayerIObject player = parameter.target.GetComponent<PlayerIObject>();
        if (player != null)
            player.Wound(parameter.attackDamage);
    }
}

