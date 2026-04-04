using UnityEngine;

/// <summary>
/// 子弹物理模拟减速组件：基于真实物理原理实现子弹击中后的减速效果
/// 设计理念：模拟动能守恒定律，子弹击中目标后能量传递
/// 机械工程类比：汽车碰撞测试 - 速度损失与碰撞角度、材质硬度相关
/// </summary>
public class BulletPhysicsSlowdown : MonoBehaviour
{
    [Header("物理参数设置")]
    [SerializeField] public float kineticEnergyLoss = 0.7f;  // 动能损失比例 (0-1)
    [SerializeField] public float materialHardness = 0.5f;   // 目标材质硬度 (0-1)
    [SerializeField] public float elasticityFactor = 0.3f;   // 弹性系数 (0-1)
    
    [Header("碰撞角度影响")]
    [SerializeField] public AnimationCurve angleImpactCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.2f);
    [SerializeField] public bool enableAngleCalculation = true;
    
    [Header("减速效果")]
    [SerializeField] public float slowdownDuration = 0.15f; // 减速持续时间
    [SerializeField] private AnimationCurve slowdownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.3f);
    
    [Header("视觉反馈")]
    [SerializeField] private ParticleSystem physicsHitEffect; // 物理碰撞特效
    [SerializeField] private float deformationScale = 0.8f;   // 子弹变形比例
    [SerializeField] private Color impactColor = Color.red;   // 碰撞颜色变化
    
    private Bullet _bullet;           // 子弹主脚本引用
    private float _originalSpeed;     // 原始速度
    private float _slowdownTimer = 0f;
    private bool _isSlowingDown = false;
    private Vector3 _originalScale;   // 原始尺寸
    private Color _originalColor;     // 原始颜色
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        _bullet = GetComponent<Bullet>();
        if (_bullet == null)
        {
            Debug.LogWarning($"[{name}] 未找到Bullet组件，物理减速效果将无法工作");
            return;
        }
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
        
        _originalSpeed = _bullet.moveSpeed;
        _originalScale = transform.localScale;
        
        Debug.Log($"[{name}] 物理减速组件初始化完成，原始速度: {_originalSpeed}");
    }
    
    private void Update()
    {
        if (!_isSlowingDown) return;
        
        // 更新减速计时
        _slowdownTimer -= Time.deltaTime;
        
        if (_slowdownTimer <= 0f)
        {
            EndPhysicsSlowdown();
            return;
        }
        
        // 根据进度计算当前速度
        float progress = 1f - (_slowdownTimer / slowdownDuration);
        float currentSpeedMultiplier = slowdownCurve.Evaluate(progress);
        
        // 应用物理减速效果
        ApplyPhysicsSlowdown(currentSpeedMultiplier);
    }
    
    /// <summary>
    /// 触发物理模拟减速效果
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    /// <param name="targetMaterial">目标材质类型</param>
    public void TriggerPhysicsSlowdown(Collision2D collision, MaterialType targetMaterial = MaterialType.Normal)
    {
        if (_bullet == null || _isSlowingDown) return;
        
        // 计算物理减速强度
        float intensity = CalculatePhysicsIntensity(collision, targetMaterial);
        
        _slowdownTimer = slowdownDuration * intensity;
        _isSlowingDown = true;
        
        // 播放物理碰撞特效
        PlayPhysicsHitEffect(collision);
        
        Debug.Log($"[{name}] 物理减速触发: 强度={intensity:F2}, 持续时间={_slowdownTimer:F3}s");
    }
    
    /// <summary>
    /// 根据节奏判定等级触发物理减速
    /// </summary>
    public void TriggerPhysicsSlowdownByRank(RhythmRank rank, Collision2D collision = null)
    {
        float intensity = GetIntensityByRank(rank);
        
        // 如果有碰撞信息，结合物理计算
        if (collision != null && enableAngleCalculation)
        {
            float physicsIntensity = CalculatePhysicsIntensity(collision, MaterialType.Normal);
            intensity = Mathf.Max(intensity, physicsIntensity); // 取较大值
        }
        
        TriggerPhysicsSlowdown(collision, MaterialType.Normal);
    }
    
    /// <summary>
    /// 计算物理减速强度（核心物理算法）
    /// </summary>
    private float CalculatePhysicsIntensity(Collision2D collision, MaterialType targetMaterial)
    {
        float baseIntensity = kineticEnergyLoss; //
        
        // 材质硬度影响
        float materialFactor = GetMaterialFactor(targetMaterial);
        baseIntensity *= materialFactor;
        
        // 碰撞角度影响
        if (enableAngleCalculation && collision != null)
        {
            float angleImpact = CalculateAngleImpact(collision);
            baseIntensity *= angleImpact;
        }
        
        // 弹性系数修正
        baseIntensity *= (1f - elasticityFactor);
        
        return Mathf.Clamp01(baseIntensity);
    }
    
    /// <summary>
    /// 计算碰撞角度对减速的影响
    /// </summary>
    private float CalculateAngleImpact(Collision2D collision)
    {
        if (collision.contactCount == 0) return 1f;
        
        // 计算子弹方向与碰撞法线的角度
        Vector2 bulletDirection = transform.right;
        Vector2 collisionNormal = collision.contacts[0].normal;
        
        float impactAngle = Vector2.Angle(bulletDirection, collisionNormal);
        float normalizedAngle = impactAngle / 180f; // 归一化到0-1
        
        // 使用曲线计算角度影响
        return angleImpactCurve.Evaluate(normalizedAngle);
    }
    
    /// <summary>
    /// 根据材质类型获取硬度系数
    /// </summary>
    private float GetMaterialFactor(MaterialType material)
    {
        return material switch
        {
            MaterialType.Soft => 0.3f,    // 软材质：减速效果弱
            MaterialType.Normal => 0.6f,   // 普通材质：中等减速
            MaterialType.Hard => 0.9f,     // 硬材质：强减速
            MaterialType.Metal => 1.2f,    // 金属：极强减速
            _ => 0.6f
        };
    }
    
    /// <summary>
    /// 应用物理减速效果
    /// </summary>
    private void ApplyPhysicsSlowdown(float speedMultiplier)
    {
        // 应用速度减速
        _bullet.moveSpeed = (int)(_originalSpeed * speedMultiplier);
        
        // 应用物理变形效果
        ApplyPhysicsDeformation(speedMultiplier);
    }
    
    /// <summary>
    /// 应用物理变形效果（子弹撞击后的形变）
    /// </summary>
    private void ApplyPhysicsDeformation(float speedMultiplier)
    {
        // 计算变形比例
        float deformation = 1f - (deformationScale * (1f - speedMultiplier));
        
        // 应用尺寸变形（子弹被压扁）
        Vector3 deformedScale = _originalScale;
        deformedScale.x *= deformation;  // 水平方向压缩
        deformedScale.y *= (1f + (1f - deformation) * 0.3f); // 垂直方向略微拉伸
        transform.localScale = deformedScale;
        
        // 应用颜色变化（撞击发热）
        if (_spriteRenderer != null)
        {
            float colorBlend = 1f - speedMultiplier;
            _spriteRenderer.color = Color.Lerp(_originalColor, impactColor, colorBlend);
        }
    }
    
    /// <summary>
    /// 结束物理减速效果
    /// </summary>
    private void EndPhysicsSlowdown()
    {
        _isSlowingDown = false;
        
        // 恢复原始速度
        _bullet.moveSpeed = (int)_originalSpeed;
        
        // 恢复原始尺寸和颜色
        transform.localScale = _originalScale;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
        
        Debug.Log($"[{name}] 物理减速结束，恢复速度: {_originalSpeed}");
    }
    
    /// <summary>
    /// 播放物理碰撞特效
    /// </summary>
    private void PlayPhysicsHitEffect(Collision2D collision)
    {
        if (physicsHitEffect != null && collision != null)
        {
            // 在碰撞点生成特效
            Vector2 hitPoint = collision.contacts[0].point;
            ParticleSystem effect = Instantiate(physicsHitEffect, hitPoint, Quaternion.identity);
            
            // 根据碰撞强度调整特效参数
            var main = effect.main;
            main.startSpeed = 5f * CalculatePhysicsIntensity(collision, MaterialType.Normal);
            
            effect.Play();
            Destroy(effect.gameObject, 3f);
        }
    }
    
    /// <summary>
    /// 根据节奏判定等级获取强度
    /// </summary>
    private float GetIntensityByRank(RhythmRank rank)
    {
        // 完美判定时物理效果更强（能量传递更充分）
        return rank switch
        {
            RhythmRank.Perfect => 1.0f,
            RhythmRank.Great => 0.8f,
            RhythmRank.Good => 0.6f,
            _ => 0.4f // Miss或其他情况
        };
    }
    
    /// <summary>
    /// 获取当前是否处于减速状态
    /// </summary>
    public bool IsSlowingDown => _isSlowingDown;
    
    /// <summary>
    /// 获取当前速度乘数
    /// </summary>
    public float CurrentSpeedMultiplier => _isSlowingDown ? (_bullet.moveSpeed / _originalSpeed) : 1f;
}

/// <summary>
/// 材质类型枚举：定义不同材质的物理特性
/// </summary>
public enum MaterialType
{
    Soft,       // 软材质：布料、肉体等
    Normal,     // 普通材质：木材、塑料等
    Hard,       // 硬材质：石头、混凝土等
    Metal       // 金属材质：钢铁、合金等
}