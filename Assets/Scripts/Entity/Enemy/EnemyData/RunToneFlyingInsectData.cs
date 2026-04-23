using UnityEngine;

/// <summary>
/// 跑调飞虫数据
/// </summary>
[CreateAssetMenu(fileName = "RunToneFlyingInsectData", menuName = "Enemy/RunToneFlyingInsect Data")]
public class RunToneFlyingInsectData : EnemyData
{
    [Header("跑调飞虫特有属性")]
    public float detectionRange = 10f;           // 检测范围
    public float attackDamage = 10f;             // 攻击力（冲撞伤害）
    public float chargeSpeed = 8f;               // 冲撞速度
    public float chargeCooldown = 2f;            // 冲撞冷却
    public float warningDuration = 0.5f;         // 预警时间
    public float stunDuration = 2f;              // 眩晕时间
    public Color warningColor = Color.red;       // 预警变色

    [Header("视觉配置")]
    public Color baseColor = Color.white;
    public Color glitchColor = new Color(1f, 0.2f, 0.2f);
    public float glitchIntensity = 0.3f;
}