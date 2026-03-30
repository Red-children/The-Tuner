using UnityEngine;

/// <summary>
/// 跑调飞虫数据
/// </summary>
[CreateAssetMenu(fileName = "RunToneFlyingInsectData", menuName = "Enemy/RunToneFlyingInsect")]
public class RunToneFlyingInsectData : EnemyData
{
    [Header("跑调飞虫特有属性")]
    public float detectionRange = 10f; // 检测范围
    public float rhythmMoveMultiplier = 1.2f; // 节奏移动倍率
    public float minMoveSpeed = 1.5f; // 最小移动速度
    public float maxMoveSpeed = 3f; // 最大移动速度
    public float attackDamage = 10; //攻击力


    [Header("行为配置")]
    public float idleMovementSpeed = 0.1f; // idle移动速度
    public float idleCircleRadius = 1f; // idle绕圈半径
    
    [Header("视觉配置")]
    public Color baseColor = new Color(0.2f, 0.2f, 0.2f); // 基础颜色（黑白噪点）
    public Color glitchColor = new Color(1f, 0.2f, 0.2f); // 故障边缘颜色
    public float glitchIntensity = 0.3f; // 故障效果强度
}
