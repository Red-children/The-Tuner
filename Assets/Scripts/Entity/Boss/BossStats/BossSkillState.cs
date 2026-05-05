using System.Collections;
using UnityEngine;

public class BossSkillState : EnemyStateBase
{
    private BossData bossData;

    public BossSkillState(FSM manager) : base(manager) { }

    public override void OnStart()
    {
        bossData = data as BossData;
        manager.animator.SetTrigger("Skill");
        controller.StartCoroutine(SkillRoutine());
    }

    private IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (runtime.target != null)
        {
            PerformSkill();
        }

        yield return new WaitForSeconds(1f);

        if (runtime.target != null)
            manager.ChangeState(StateType.Chase);
        else
            manager.ChangeState(StateType.Idle);
    }

    private void PerformSkill()
    {
        BossSkill skill = controller.GetComponent<BossSkill>();
        if (skill != null)
        {
            skill.ExecuteSkill();
        }
        else
        {
            Debug.LogWarning($"[{controller.name}] 未找到 BossSkill 组件，技能释放失败");
        }
    }

    public override void OnUpdate() { }
    public override void OnExit() { }
}
