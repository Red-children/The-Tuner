using System.Collections;
using UnityEngine;

/// <summary>
/// 低音炮
/// </summary>
public class BassCannon : WeaponInfo
{
    [Header("蓄力参数")]
    private bool isCharging = false;
    private float chargeTimer = 0f;

    // 蓄力振动参数
    private static readonly Vector2[] shakeDirections = new Vector2[]
    {
        new Vector2(1f, 0.5f),
        new Vector2(-1f, 0.3f),
        new Vector2(0.3f, 1f),
        new Vector2(-0.5f, -1f),
        new Vector2(1f, -0.3f),
        new Vector2(-0.3f, -0.7f),
        new Vector2(0.7f, 0.7f),
        new Vector2(-1f, 0.8f),
    };
    private int directionIndex = 0;
    private float directionSwitchTimer = 0f;
    private float directionSwitchInterval = 0.15f;

    /// <summary>
    /// 处理开火输入
    /// </summary>
    public override void HandleFireInput()
    {
    }
    /// <summary>
    /// 开始蓄力
    /// </summary>
    public void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;
        directionIndex = 0;
        directionSwitchTimer = 0f;
    }

    /// <summary>
    /// 释放蓄力
    /// </summary>
    public void ReleaseCharge()
    {
        if (!isCharging) return;

        EventBus.Instance.Trigger(new PlayerAtkEvent());

        float progress = Mathf.Clamp01(chargeTimer / weaponStats.chargeTime);

        if (progress >= 1.0f)
        {
            SelfDamage(weaponStats.perfectChargeDamage * weaponStats.selfDamageRatio);
            EventBus.Instance.Trigger(new CameraShakeEvent { intensity = 0.2f });
        }
        else if (progress >= weaponStats.overchargeThreshold)
        {
            var rank = RhythmManager.Instance.GetRank();
            float finalDamage = rank.rank == RhythmRank.Perfect
                              ? weaponStats.perfectChargeDamage
                              : weaponStats.missChargeDamage;

            SpawnChargeProjectile(finalDamage, rank.rank == RhythmRank.Perfect);
            EventBus.Instance.Trigger(new CameraShakeEvent { intensity = (rank.rank == RhythmRank.Perfect)? 0.4f : 0.15f });
        }
        else
        {
            // 弱蓄力释放 伤害计算公式 （基础伤害 * 弱蓄力伤害倍率）
            SpawnChargeProjectile(weaponStats.damage * weaponStats.weakReleaseDamageRatio, false);
        }

        isCharging = false;
        chargeTimer = 0f;
    }

    /// <summary>
    /// 生成蓄力子弹
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="isPerfect">是否完美</param>
    private void SpawnChargeProjectile(float damage, bool isPerfect)
    {
        GameObject bulletObj = Instantiate(weaponStats.bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(damage);

            if (isPerfect)
            {
                bullet.EnablePenetration(5, 0.1f);
                SpriteRenderer sr = bulletObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 0.6f, 0f);
            }
        }
    }
    /// <summary>
    /// 自伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    private void SelfDamage(float damage)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerAPI playerAPI = playerObj.GetComponent<PlayerAPI>();
            if (playerAPI != null)
                playerAPI.TakeDamage(Mathf.RoundToInt(damage));
        }
    }

    private void Update()
    {
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(chargeTimer / weaponStats.chargeTime);

            // 随蓄力进度，方向切换越来越快
            float interval = Mathf.Lerp(0.2f, 0.06f, progress);
            directionSwitchTimer += Time.deltaTime;
            //持续切换方向
            if (directionSwitchTimer >= interval)
            {
                directionSwitchTimer = 0f;
                directionIndex = (directionIndex + 1) % shakeDirections.Length;
            }

            // 正弦波低频脉动：蓄力初期低频，后期高频
            float pulseFreq = Mathf.Lerp(3f, 12f, progress);
            float pulse = (Mathf.Sin(chargeTimer * pulseFreq * Mathf.PI * 2f) + 1f) * 0.5f;

            // 强度 = 基础强度 * 脉动
            float baseIntensity = Mathf.Lerp(weaponStats.chargeShakeBase, weaponStats.chargeShakeMax, progress);
            float pulseIntensity = baseIntensity * (0.3f + 0.7f * pulse);

            // 使用方向数组，让蓄力时摄像机在不同方向来回摆动
            Vector2 dir = shakeDirections[directionIndex];
            EventBus.Instance.Trigger(new CameraShakeEvent { intensity = pulseIntensity });
        }
    }
}