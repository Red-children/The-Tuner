using System.Collections;
using UnityEngine;

public class EnemyMeleeAttackState : EnemyStateBase
{
    private Coroutine attackCoroutine;
    private Transform attackTarget; // 攻击时锁定的目标，防止中途丢失

    public EnemyMeleeAttackState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        Debug.Log("进入近战攻击状态");
        attackTarget = runtime.target; // 记录当前目标
        if (attackTarget == null)
        {
            // 如果没有目标，直接退出
            manager.ChangeState(StateType.Patrol);
            return;
        }

        // 播放攻击动画
        controller.animator.SetTrigger("Attack");
        // 启动攻击协程
        if (attackCoroutine != null) controller.StopCoroutine(attackCoroutine);
        attackCoroutine = controller.StartCoroutine(AttackCoroutine());
    }

    public override void OnUpdate()
    {
        // 只检查受伤或目标死亡（因为目标可能被其他系统杀死）
        if (runtime.getHit)
        {
            manager.ChangeState(StateType.Wound);
            return;
        }
        // 如果攻击目标已死亡或消失，提前退出攻击状态
        if (attackTarget == null || attackTarget.gameObject == null)
        {
            manager.ChangeState(StateType.Patrol);
            return;
        }
        // 不在此处检查距离，防止动画被打断
    }

    private IEnumerator AttackCoroutine()
    {
        // 等待打击时刻（根据动画调整，这里假设0.3秒后命中）
        yield return new WaitForSeconds(0.3f);

        // 造成伤害
        if (IsTargetInRange())
        {
            PlayerAPI player = attackTarget.GetComponent<PlayerAPI>();
            if (player != null)
            {
                int damage = (data as MeleeEnemyData).attackDamage;
                player.TakeDamage(damage);
                Debug.Log($"近战攻击造成伤害 {damage}");
            }
        }
        

        // 等待动画结束（假设动画总长0.8秒，减去已等待的0.3秒，再等0.5秒）
        yield return new WaitForSeconds(0.5f);

        // 攻击结束，切换到追逐状态
        manager.ChangeState(StateType.Chase);
    }

    private bool IsTargetInRange()
    {
        if (attackTarget == null) return false;
        Vector2 attackWorldPos = controller.GetAttackWorldPos();
        MeleeEnemyData meleeData = data as MeleeEnemyData;
        if (meleeData == null) return false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackWorldPos, meleeData.attackRange, meleeData.targetLayer);
        foreach (var hit in hits)
            if (hit.transform == attackTarget) return true;
        return false;
    }

    public override void OnExit()
    {
        if (attackCoroutine != null) controller.StopCoroutine(attackCoroutine);
    }
}