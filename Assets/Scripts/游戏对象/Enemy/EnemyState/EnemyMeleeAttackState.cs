using System.Collections;
using UnityEngine;

/// <summary>
/// 敌人近战攻击状态类，负责处理敌人在近战攻击时的行为逻辑，包括攻击动画、伤害判定以及攻击结束后的状态切换等，根据敌人类型（近战或远程）进行不同的处理。
/// </summary>
public class EnemyMeleeAttackState : EnemyStateBase
{
    private Coroutine attackCoroutine;
    private Transform attackTarget; // 记录当前攻击目标的Transform

    public EnemyMeleeAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {

        attackTarget = runtime.target; // 记录当前攻击目标的Transform
        if (attackTarget == null)
        {
            // 如果没有目标，直接切换回巡逻状态
            manager.ChangeState(StateType.Patrol);
            return;
        }

        // 触发攻击动画
        //controller.animator.SetTrigger("Attack");
        if (attackCoroutine != null) controller.StopCoroutine(attackCoroutine);
        attackCoroutine = controller.StartCoroutine(AttackCoroutine());
    }

    public override void OnUpdate()
    {
        // 如果受到攻击，立即切换到受伤状态
        if (runtime.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }
        // 如果攻击木有目标或者目标已经无效，切换回巡逻状态
        if (attackTarget == null || attackTarget.gameObject == null)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }

    }

/// <summary>
/// 攻击协程，负责处理攻击的时序逻辑，包括攻击动画的播放、伤害判定以及攻击结束后的状态切换等，根据敌人类型（近战或远程）进行不同的处理。
/// </summary>
/// <returns></returns>
    private IEnumerator AttackCoroutine()
    {
       // 等待攻击动画的关键帧，确保伤害判定在正确的时机进行，这里假设攻击动画的关键帧在0.3秒处，可以根据实际动画调整这个时间
        yield return new WaitForSeconds(0.3f);

        // 攻击判定，检查目标是否在攻击范围内，如果在范围内则对目标造成伤害
        if (IsTargetInRange())
        {
            PlayerAPI player = attackTarget.GetComponent<PlayerAPI>();
            if (player != null)
            {
                int damage = (data as MeleeEnemyData).attackDamage;
                player.TakeDamage(damage);

            }
        }


        //攻击结束后等待一段时间，确保攻击动画播放完成，然后切换回追逐状态继续追击玩家
        yield return new WaitForSeconds(0.5f);

        // 攻击结束后切换回追逐状态继续追击玩家
        manager.ChangeState(StateType.Chase);
    }

    /// <summary>
    /// 检查攻击目标是否在攻击范围内，使用OverlapCircle检测目标是否在攻击范围内，确保敌人能够正确地判断何时可以攻击玩家，根据敌人类型（近战或远程）进行不同的处理。
    /// </summary>
    /// <returns></returns>
    private bool IsTargetInRange()
    {
        if (attackTarget == null) return false;
        Vector2 attackWorldPos = controller.GetAttackWorldPos();    //得到攻击点的世界坐标
        MeleeEnemyData meleeData = data as MeleeEnemyData;          //将敌人数据转换为近战敌人数据，获取攻击范围和目标层级等信息
        if (meleeData == null) return false;
        // 使用OverlapCircle检测目标是否在攻击范围内，确保敌人能够正确地判断何时可以攻击玩家
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackWorldPos, meleeData.attackRange, meleeData.targetLayer);
        foreach (var hit in hits)
        // 如果检测到的碰撞体是当前攻击目标，则认为目标在攻击范围内，返回true
        //  注意这里有个问题多个玩家被一个敌人攻击时会有问题，后续可以改成只攻击一个玩家或者增加优先级机制
            if (hit.transform == attackTarget) return true;
        return false;
    }

    public override void OnExit()
    {
        // 停止攻击协程，确保在状态切换时不会继续执行攻击逻辑，避免出现攻击动画和伤害判定的混乱等问题
        if (attackCoroutine != null) controller.StopCoroutine(attackCoroutine);
    }
}