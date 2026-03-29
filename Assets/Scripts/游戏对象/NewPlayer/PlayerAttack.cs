using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerWeapon playerWeapon;
    private PlayerStats stats;

    [Header("��ս��������")]
    public float meleeRange = 1.5f;
    public int meleeDamage = 20;
    public float meleeCoolDown = 0.5f;
    private double lastMeleeTime = -999f;
    private double lastShootTime = -999f;
    public float shootCoolDown = 0.1f; // 射击冷却时间

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    private void Update()
    {
        // 射击检测 - 使用GetMouseButtonDown捕捉点击瞬间
        if (Input.GetMouseButtonDown(0))
        {
            double currentTime = AudioSettings.dspTime;
            if (currentTime > lastShootTime + shootCoolDown)
            {
                lastShootTime = currentTime;
                var weapon = playerWeapon.GetCurrentWeapon();
                if (weapon != null)
                {
                    // 确保RhythmManager存在
                    float rhythmMultiplier = 1f;
                    if (RhythmManager.Instance != null)
                    {
                        rhythmMultiplier = RhythmManager.Instance.GetRank(currentTime).multiplier;
                    }
                    weapon.Shoot(stats.TotalAttack, rhythmMultiplier);
                    Debug.Log("[PlayerAttack] 开枪成功，武器：" + weapon.name + "，倍率：" + rhythmMultiplier);
                }
                else
                {
                    Debug.LogWarning("[PlayerAttack] 没有找到武器");
                }
                EventBus.Instance.Trigger(new PlayerAtkEvent());
                EventBus.Instance.Trigger(new CameraShakeEvent());
            }
            else
            {
                Debug.Log("[PlayerAttack] 射击冷却中");
            }
        }

        // 近战攻击
        if (Input.GetKeyDown(KeyCode.V))
        {
            double currentTime = AudioSettings.dspTime;
            if (currentTime > lastMeleeTime + meleeCoolDown)
            {
                EventBus.Instance.Trigger(new PlayerAtkEvent());
                EventBus.Instance.Trigger(new CameraShakeEvent());
                MeleeAttack();
            }
        }
    }

    private void MeleeAttack()
    {
        lastMeleeTime = AudioSettings.dspTime;

        // 获取节奏倍率
        float multiplier = GetRhythmMultiplier();

        float finalDamage = (GetPlayerAttack() + meleeDamage) * multiplier;

        // 范围攻击
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.Wound(finalDamage);
        }

        // 触发近战事件（用于特效等）
        EventBus.Instance.Trigger(new PlayerMeleeEvent { damage = finalDamage, hitPoint = transform.position });
    }

    // 实时获取玩家攻击力的方法（从 PlayerStats 读取）
    private float GetPlayerAttack()
    {
        // 从 PlayerStats 获取
        PlayerStats stats = GetComponent<PlayerStats>();
        return stats != null ? stats.TotalAttack : 0;
    }

    private float GetRhythmMultiplier()
    {
        // 从 RhythmManager 实时获取
        if (RhythmManager.Instance != null)
        {
            var rank = RhythmManager.Instance.GetRank(AudioSettings.dspTime);
            return rank.multiplier;
        }
        return 1f;
    }
}