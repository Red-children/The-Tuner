using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class EnemyRangedAttackState : EnemyStateBase
{
    private float lastAttackTime = 0f;
    private float attackCooldown = 1.5f;
    private bool attackFinished;
    private int currentAttackIndex; // 本次攻击选择的类型索引

    public EnemyRangedAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        // 冷却判断：如果还在冷却中，不进入攻击状态
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log($"[{controller.name}] 攻击冷却中，返回接近状态");
            manager.ChangeState(StateType.Approach);
            return;
        }

        attackFinished = false;

        // 随机选择攻击类型
        RangedEnemyData rangedData = controller.data as RangedEnemyData;
        if (rangedData != null && rangedData.attackAnimationIndex != null && rangedData.attackAnimationIndex.Length > 0)
        {
            currentAttackIndex = UnityEngine.Random.Range(0, rangedData.attackAnimationIndex.Length);
        }
        else
        {
            currentAttackIndex = 0;
        }

        // 通知控制器当前攻击类型（用于特效选择）
        controller.currentAttackIndex = currentAttackIndex;

        // 设置动画参数，选择对应的攻击动画
        manager.animator.SetInteger("AttackIndex", currentAttackIndex);
        manager.animator.SetTrigger("Attack");
        Debug.Log($"[{controller.name}] 进入远程攻击状态，攻击类型: {currentAttackIndex}");
    }

    public override void OnUpdate()
    {
        // 如果敌人受到攻击，立即切换到受伤状态
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
        // 如果攻击没有目标或者目标已经无效，切换回巡逻状态
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }
        
        // 计算敌人与目标之间的距离，并根据距离判断是否需要切换状态
        float distance = Vector2.Distance(manager.transform.position, runtime.target.position);
        float attackRange = (controller.data as RangedEnemyData).attackRange;

        // 距离过远切换到接近状态（与接近状态判断一致）
        if (distance > attackRange)
        {
            manager.ChangeState(StateType.Approach);
            return;
        }

        // 检查武器是否存在
        if (controller.weapon == null)
        {
            Debug.LogWarning($"[{controller.name}] 远程敌人缺少武器组件");
            manager.ChangeState(StateType.Approach);
            return;
        }

        // 直接瞄准目标方向（不转向）
        Vector2 direction = (runtime.target.position - controller.transform.position).normalized;
        
        // 根据方向设置武器朝向
        if (controller.weapon.firePoint != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            controller.weapon.firePoint.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public override void OnExit() { }

    // 由动画事件（经Controller转发）调用，执行远程攻击
    public void OnComboHit()
    {
        
        if (attackFinished) return;

        if (controller.weapon == null)
        {
            Debug.LogWarning($"[{controller.name}] 远程敌人缺少武器组件，无法执行攻击");
            return;
        }

        // 根据攻击类型选择伤害值
        RangedEnemyData rangedData = controller.data as RangedEnemyData;
        float damage = rangedData != null ? rangedData.Atk : 10f;
        if (rangedData != null && rangedData.attackDamages != null && currentAttackIndex < rangedData.attackDamages.Length)
        {
            damage = rangedData.attackDamages[currentAttackIndex];
        }

        Debug.Log($"[{controller.name}] 动画事件触发远程攻击，攻击类型: {currentAttackIndex}，伤害: {damage}");
        controller.weapon.Shoot(damage, 1, RhythmRank.Good);
        lastAttackTime = Time.time;
    }

    // 由动画事件调用，结束攻击状态
    public void OnAttackFinished()
    {
        if (attackFinished) return;
        attackFinished = true;
        Debug.Log($"[{controller.name}] 远程攻击结束，返回接近状态");
        manager.ChangeState(StateType.Patrol);
    }
}