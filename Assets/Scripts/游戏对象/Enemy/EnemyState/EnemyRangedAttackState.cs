using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class EnemyRangedAttackState : EnemyStateBase
{
    public EnemyRangedAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        Debug.Log("敌人进入远程攻击状态");
    }

    public override void OnUpdate()
    {
        // 如果敌人受到攻击，立即切换到受伤状态
        if (runtime.getHit) { manager.ChangeState(StateType.Wound); return; }
        // 如果攻击没有目标或者目标已经无效，切换回巡逻状态
        if (runtime.target == null) { manager.ChangeState(StateType.Patrol); return; }
        // 计算敌人与目标之间的距离，并根据距离判断是否需要切换状态，例如如果目标距离过远或者过近，切换到接近状态，确保敌人能够在合理的范围内进行攻击，同时根据与玩家的距离动态调整移动方向，形成更智能的攻击行为
        float distance = Vector2.Distance(manager.transform.position, runtime.target.position);
        float attackRange = (controller.data as RangedEnemyData).attackRange;

        //距离过近或者过远都切换到接近状态，确保敌人能够在合理的范围内进行攻击，同时根据与玩家的距离动态调整移动方向，形成更智能的攻击行为
        if (distance > attackRange || distance < attackRange * 0.6f)
        {
            manager.ChangeState(StateType.Approach);
            return;
        }

        // 执行攻击逻辑，例如调用敌人的攻击方法，确保敌人能够正确地执行远程攻击行为
        if (controller.weapon != null)
        {
            // 根据敌人数据中的攻击参数调用攻击方法，例如攻击力、攻击范围等，确保敌人能够根据设定的参数进行合理的攻击行为
            //敌人的攻击倍率默认为good
            controller.weapon.Shoot(((controller.data) as RangedEnemyData).Atk, 1, RhythmRank.Good);
        }
    }

    public override void OnExit() { }

}