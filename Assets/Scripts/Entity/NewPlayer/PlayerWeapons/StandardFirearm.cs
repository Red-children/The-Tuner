using System.Collections;
using UnityEngine;

public class StandardFirearm : WeaponInfo
{
    /// <summary>
    /// 处理武器输入
    /// </summary>
    public override void HandleFireInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryFire();
        }
    }

    /// <summary>
    /// 武器开火
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="rhythmMultiplier">节奏乘数</param>
    /// <param name="rank">节奏等级</param>
    public override void Fire(float damage, float rhythmMultiplier, RhythmRank rank)
    {
        if (isReloading) return;        //武器 正在装填返回
        if (weaponStats == null) return;//武器数据为空返回

        double currentTime = AudioSettings.dspTime;//记录当前时间
        if (currentTime < lastShootTime + weaponStats.fireRate) return;//射击间隔过短返回

        //子弹不足启动自动装填然后返回
        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        //如果是是玩家的武器
        if (owner == WeaponOwner.Player)
        {
            //触发子弹变化事件 提供UI监听
            EventBus.Instance.Trigger(new AmmoChangedEvent
            {
                currentAmmo = currentAmmo,
                reserveAmmo = -1,
                weaponId = weaponStats.id
            });
        }
        //伤害计算 公式为（玩家伤害+武器伤害）*节奏倍率
        float finalDamage = (damage + weaponStats.damage) * rhythmMultiplier;
        //触发屏幕振动
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = weaponStats.shakeIntensity });
        //生成子弹
        SpawnBullet(finalDamage, rank);

        //扣除子弹
        currentAmmo--;
        //记录开火事件用于下一次开火间隔的判断-
        lastShootTime = currentTime;
    }

    /// <summary>
    /// 尝试开火
    /// </summary>
    private void TryFire()
    {
        //获取当前节奏等级和节奏倍率
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