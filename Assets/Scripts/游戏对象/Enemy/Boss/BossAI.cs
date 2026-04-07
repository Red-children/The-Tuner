using UnityEngine;

public class BossAI : MonoBehaviour
{
    public BossEnemy boss;
    public BossSkill skill;
    public Transform target;

    public float decisionInterval = 0.5f;
    private float timer;

    void Start()
    {
        if (boss == null)
            boss = GetComponent<BossEnemy>();

        if (skill == null)
            skill = GetComponent<BossSkill>();
    }

    void Update()
    {
        if (boss == null || target == null || skill == null) return;

        timer += Time.deltaTime;

        if (timer >= decisionInterval)
        {
            timer = 0f;
            Decide();
        }
    }

    void Decide()
    {
        float distance = Vector3.Distance(boss.transform.position, target.position);

        // 优先技能
        if (skill.CanUseSkill())
        {
            skill.UseRandomSkill(boss, target);
            skill.OnUseSkill();
            return;
        }

        // 普通攻击
        if (distance <= boss.attackRange)
        {
            boss.Attack();
        }
    }
}