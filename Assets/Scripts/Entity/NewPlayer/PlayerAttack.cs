using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 玩家攻击模块
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Melee Attack")]
    public float meleeRange = 1.5f;
    public int meleeDamage = 20;
    public float meleeCoolDown = 0.5f;

    private PlayerWeapon playerWeapon;
    private PlayerStats stats;
    private double lastMeleeTime = -999f;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    private void Update()
    {
        var weapon = playerWeapon?.GetCurrentWeapon();

        if (weapon != null)
        {
            weapon.HandleFireInput();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            TryMelee();
        }
    }

    private void TryMelee()
    {
        double currentTime = AudioSettings.dspTime;
        if (currentTime <= lastMeleeTime + meleeCoolDown)
        {
            return;
        }

        lastMeleeTime = currentTime;
        var rhythmResult = SampleRhythm(currentTime, "Melee");

        EventBus.Instance.Trigger(new PlayerAtkEvent());
        EventBus.Instance.Trigger(new CameraShakeEvent());
        MeleeAttack(rhythmResult.multiplier);
    }

    private void MeleeAttack(float rhythmMultiplier)
    {
        float finalDamage = (stats.TotalAttack + meleeDamage) * rhythmMultiplier;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange, LayerMask.GetMask("Enemy"));

        bool hitEnemy = false;
        RhythmRank highestRank = RhythmRank.Miss;

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                if (enemy is EnemyController ec)
                    ec.SetAttackerPosition(transform.position);

                // 获取当前节奏判定等级
                var rhythmResult = SampleRhythm(AudioSettings.dspTime, "Melee");
                enemy.Wound(finalDamage, rhythmResult.rank);
                hitEnemy = true;
                highestRank = (RhythmRank)Mathf.Max((int)highestRank, (int)rhythmResult.rank);

                // 触发敌人卡肉感
                if (HitStopManager.Instance != null)
                {
                    HitStopManager.Instance.TriggerEnemyHitStop(hit.gameObject, rhythmResult.rank, finalDamage);
                }
            }
        }

        // 如果命中敌人，触发玩家卡肉感（轻微反馈）
        if (hitEnemy && HitStopManager.Instance != null)
        {
            HitStopManager.Instance.TriggerPlayerHitStop(highestRank, finalDamage);
        }

        EventBus.Instance.Trigger(new PlayerMeleeEvent
        {
            damage = finalDamage,
            hitPoint = transform.position
        });
    }

    private RhythmManager.RankResult SampleRhythm(double inputDspTime, string source)
    {
        if (RhythmManager.Instance == null)
        {
            return new RhythmManager.RankResult
            {
                rank = RhythmRank.Miss,
                multiplier = 1f,
                isInWindow = false,
                judgedDspTime = inputDspTime,
                referenceBeatTime = inputDspTime,
                offsetSeconds = 0,
                offsetMilliseconds = 0
            };
        }

        var gameplayResult = RhythmManager.Instance.GetRankAtTime(inputDspTime);
        var debugResult = RhythmManager.Instance.GetDebugRankAtTime(inputDspTime);
        EventBus.Instance.Trigger(new RhythmInputDebugEvent
        {
            inputDspTime = inputDspTime,
            judgedDspTime = debugResult.judgedDspTime,
            referenceBeatTime = debugResult.referenceBeatTime,
            offsetSeconds = debugResult.offsetSeconds,
            offsetMilliseconds = debugResult.offsetMilliseconds,
            rank = debugResult.rank,
            multiplier = gameplayResult.multiplier,
            isInWindow = debugResult.isInWindow,
            source = source
        });
        return gameplayResult;
    }
}
