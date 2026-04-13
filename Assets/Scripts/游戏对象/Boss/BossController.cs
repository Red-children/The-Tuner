using System.Collections;
using UnityEngine;

public class BossController : EnemyBase
{
    public BossData bossData;

    public BossFSM manager;
    public BossRuntime runtime;
    public BossSkill skill;          
    public Animator animator;       

    public void Awake()
    {
        manager = GetComponent<BossFSM>();
        runtime = GetComponent<BossRuntime>();
        skill = GetComponent<BossSkill>();        // 保留
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();      // 保留

        if (manager == null) Debug.LogError("BossFSM组件未找到！");
        if (runtime == null) Debug.LogError("BossRuntime组件未找到！");
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer组件未找到！");

        manager.Initialize(runtime, this);
        runtime.Init(bossData);
    }

    void Start()
    {
        if (runtime.target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                runtime.target = player.transform;
        }
    }

    public override void Wound(float damage, RhythmRank rank)
    {
        if (runtime.getHit) return;
        runtime.getHit = true;
        runtime.currentHealth -= damage;

        // 保留 a.7 的动画触发
        animator?.SetTrigger("Hurt");

        ShowDamageText(transform.position, damage, rank);
        manager?.ChangeState(BossStateType.Wound); // 使用 BossStateType 统一
    }

    public override void OnKilled()
    {
        if (isDead) return;
        isDead = true;

        animator?.SetTrigger("Death");

        if (ownerRoom != null)
            ownerRoom.UnregisterEnemy(this);
        else
            Debug.LogWarning("BossController: ownerRoom未设置，无法注销敌人！");

        EventBus.Instance.Trigger(new EnemyDiedStruct(this, transform.position));
    }

    public override void ShowDamageText(Vector3 targetPosition, float damage, RhythmRank rank)
    {
        if (runtime?.DamageTextPrefab == null) return;
        GameObject dmgObj = Instantiate(runtime.DamageTextPrefab, targetPosition, Quaternion.identity);
        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
        dmgNumber?.SetDamage(damage, rank);
    }

    public override IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    protected override void UpdateBehavior() { }

    public void PlayIdle() => animator?.Play("Idle");
    public void PlayAttack() => animator?.Play("Attack");
    public void PlaySkill() => animator?.Play("Skill");
}