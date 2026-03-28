using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerWeapon playerWeapon;
    private PlayerStats stats;

    [Header("近战攻击设置")]
    public float meleeRange = 1.5f;
    public int meleeDamage = 20;
    public float meleeCoolDown = 0.5f;
    private float lastMeleeTime = -999f;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    private void Update()
    {
        // 射击（远程）
        if (Input.GetMouseButton(0))
        {
            var weapon = playerWeapon.GetCurrentWeapon();
            if (weapon != null)
                weapon.Shoot(stats.TotalAttack,RhythmManager.Instance.GetCurrentMultiplier());
            EventBus.Instance.Trigger(new CameraShakeEvent());
        }

        // 近战攻击
        if (Input.GetKeyDown(KeyCode.V) && Time.time > lastMeleeTime + meleeCoolDown)
        {
            MeleeAttack();
        }
    }

    private void MeleeAttack()
    {
        lastMeleeTime = Time.time;

        // 获取节奏倍率（假设从 PlayerStats 或事件缓存）
        float multiplier = GetRhythmMultiplier();

        float finalDamage = (GetPlayerAttack() + meleeDamage) * multiplier;

        // 范围检测
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.Wound(finalDamage);
        }

        // 可选：触发近战事件（供特效、音效等）
        EventBus.Instance.Trigger(new PlayerMeleeEvent { damage = finalDamage, hitPoint = transform.position });
    }

    // 临时获取玩家攻击力和节奏倍率的方法（可根据你的实际架构调整）
    private float GetPlayerAttack()
    {
        // 假设从 PlayerStats 获取
        PlayerStats stats = GetComponent<PlayerStats>();
        return stats != null ? stats.TotalAttack : 0;
    }

    private float GetRhythmMultiplier()
    {
        // 可以从 RhythmManager 实时获取，或从缓存的 RhythmData 中取
        // 这里简单从 RhythmManager 获取
        if (RhythmManager.Instance != null)
        {
            var rank = RhythmManager.Instance.GetRank(AudioSettings.dspTime);
            return rank.multiplier;
        }
        return 1f;
    }
}