using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class EnemyRangedAttackState : EnemyStateBase
{
    private float lastAttackTime = 0f;
    private float attackCooldown = 1.5f;

    public EnemyRangedAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        Debug.Log($"[{controller.name}] 进入远程攻击状态");
        lastAttackTime = Time.time;
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

        // 调试信息：显示武器状态
        Debug.Log($"[{controller.name}] 武器状态: weapon={controller.weapon != null}, firePoint={controller.weapon.firePoint != null}, projectilePrefab={controller.weapon.GetComponent<EnemyRangedWeapon>()?.projectilePrefab != null}");

        // 直接瞄准目标方向（不转向）
        Vector2 direction = (runtime.target.position - controller.transform.position).normalized;
        
        // 根据方向设置武器朝向
        if (controller.weapon.firePoint != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            controller.weapon.firePoint.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 执行攻击逻辑（带冷却时间）
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log($"[{controller.name}] 执行远程攻击，伤害: {((controller.data) as RangedEnemyData).Atk}");
            controller.weapon.Shoot(((controller.data) as RangedEnemyData).Atk, 1, RhythmRank.Good);
            lastAttackTime = Time.time;
        }
    }

    public override void OnExit() { }
}