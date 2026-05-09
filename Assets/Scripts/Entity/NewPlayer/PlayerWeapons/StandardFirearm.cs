using System.Collections;
using UnityEngine;

public class StandardFirearm : WeaponInfo
{
    public override void HandleFireInput()
    {
        Debug.Log("StandardFirearm: HandleFireInput called");
        TryFire();
    }

    public override void Fire(float damage, float rhythmMultiplier, RhythmRank rank)
    {
        Debug.Log($"StandardFirearm: Fire called - isReloading={isReloading}, weaponStats={weaponStats}, currentAmmo={currentAmmo}");
        
        if (isReloading) 
        {
            Debug.Log("StandardFirearm: Returning because isReloading");
            return;
        }
        if (weaponStats == null) 
        {
            Debug.LogError("StandardFirearm: weaponStats is null!");
            return;
        }

        double currentTime = AudioSettings.dspTime;
        if (currentTime < lastShootTime + weaponStats.fireRate) 
        {
            Debug.Log("StandardFirearm: Returning because of fire rate");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("StandardFirearm: Starting reload");
            StartReload();
            return;
        }

        // 播放开火特效
        PlayFireEffect();

        if (owner == WeaponOwner.Player)
        {
            Debug.Log("StandardFirearm: Triggering PlayerFiredEvent");
            EventBus.Instance.Trigger(new PlayerFiredEvent());
            EventBus.Instance.Trigger(new AmmoChangedEvent
            {
                currentAmmo = currentAmmo,
                reserveAmmo = -1,
                weaponId = weaponStats.id
            });
        }
        else
        {
            Debug.LogWarning($"StandardFirearm: owner is {owner}, not Player!");
        }

        float finalDamage = (damage + weaponStats.damage) * rhythmMultiplier;
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = weaponStats.shakeIntensity });
        SpawnBullet(finalDamage, rank);

        currentAmmo--;
        lastShootTime = currentTime;
    }

    /// <summary>
    /// 播放开火特效
    /// </summary>
    private void PlayFireEffect()
    {
        if (weaponStats.fireEffectPrefab != null && firePoint != null)
        {
            // 在开火点生成特效，沿着X轴旋转90度
            Quaternion effectRotation = firePoint.rotation * Quaternion.Euler(-90f, 0f, 0f);
            Instantiate(weaponStats.fireEffectPrefab, firePoint.position, effectRotation);
        }
    }

    private void TryFire()
    {
        var rhythmResult = SampleRhythm(AudioSettings.dspTime, "Shoot");
        float playerAttack = GetPlayerAttack();
        Fire(playerAttack, rhythmResult.multiplier, rhythmResult.rank);
    }

    /// <summary>
    /// 获取玩家伤害
    /// </summary>
    /// <returns>玩家伤害</returns>
    private float GetPlayerAttack()
    {
        PlayerStats stats = GetComponentInParent<PlayerStats>();
        return stats != null ? stats.TotalAttack : 0f;
    }

    /// <summary>
    /// 获取当前节奏等级和节奏倍率
    /// </summary>
    /// <param name="inputDspTime">输入时间</param>
    /// <param name="source">事件来源</param>
    /// <returns>当前等级和节奏倍率</returns>
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