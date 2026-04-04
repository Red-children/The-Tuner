using UnityEngine;

/// <summary>
/// 子弹碰撞减速组件：挂载在子弹上，实现击中目标后的减速效果
/// 设计理念：模拟真实物理，子弹击中目标后动能损失，速度减慢
/// </summary>
public class BulletHitSlowdown : MonoBehaviour
{
    [Header("减速设置")]
    [SerializeField] private float slowdownDuration = 0.1f; // 减速持续时间
    [SerializeField] private float slowdownFactor = 0.3f;   // 减速系数（0-1）
    [SerializeField] private AnimationCurve slowdownCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1f);
    
    [Header("视觉反馈")]
    [SerializeField] private ParticleSystem hitEffect;      // 击中特效
    [SerializeField] private float scaleReduction = 0.2f;   // 尺寸缩小比例
    
    private Bullet _bullet;           // 子弹主脚本引用
    private float _originalSpeed;     // 原始速度
    private float _slowdownTimer = 0f;
    private bool _isSlowingDown = false;
    private Vector3 _originalScale;   // 原始尺寸
    
    private void Awake()
    {
        _bullet = GetComponent<Bullet>();
        if (_bullet == null)
        {
            Debug.LogWarning($"[{name}] 未找到Bullet组件，减速效果将无法工作");
            return;
        }
        
        _originalSpeed = _bullet.moveSpeed;
        _originalScale = transform.localScale;
        
        Debug.Log($"[{name}] 子弹减速组件初始化完成，原始速度: {_originalSpeed}");
    }
    
    private void Update()
    {
        if (!_isSlowingDown) return;
        
        // 更新减速计时
        _slowdownTimer -= Time.deltaTime;
        
        if (_slowdownTimer <= 0f)
        {
            EndSlowdown();
            return;
        }
        
        // 根据进度计算当前速度
        float progress = 1f - (_slowdownTimer / slowdownDuration);
        float currentSpeedMultiplier = slowdownCurve.Evaluate(progress);
        
        // 应用减速效果
        ApplySlowdown(currentSpeedMultiplier);
    }
    
    /// <summary>
    /// 触发子弹减速效果
    /// </summary>
    /// <param name="intensity">减速强度（0-1）</param>
    public void TriggerSlowdown(float intensity = 1f)
    {
        if (_bullet == null || _isSlowingDown) return;
        
        _slowdownTimer = slowdownDuration * intensity;
        _isSlowingDown = true;
        
        // 播放击中特效
        PlayHitEffect();
        
        Debug.Log($"[{name}] 子弹触发减速: {_slowdownTimer:F3}s, 强度: {intensity:F2}");
    }
    
    /// <summary>
    /// 根据节奏判定等级触发减速
    /// </summary>
    public void TriggerSlowdownByRank(RhythmRank rank)
    {
        float intensity = GetIntensityByRank(rank);
        TriggerSlowdown(intensity);
    }
    
    private void ApplySlowdown(float speedMultiplier)
    {
        // 应用速度减速
        _bullet.moveSpeed = (int) (_originalSpeed * speedMultiplier);
        
        // 应用视觉缩放（可选）
        float scaleMultiplier = 1f - (scaleReduction * (1f - speedMultiplier));
        transform.localScale = _originalScale * scaleMultiplier;
    }
    
    private void EndSlowdown()
    {
        _isSlowingDown = false;
        
        // 恢复原始速度
        _bullet.moveSpeed = (int) _originalSpeed;
        
        // 恢复原始尺寸
        transform.localScale = _originalScale;
        
        Debug.Log($"[{name}] 子弹减速结束，恢复速度: {_originalSpeed}");
    }
    
    private void PlayHitEffect()
    {
        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f); // 2秒后自动销毁
        }
    }
    
    private float GetIntensityByRank(RhythmRank rank)
    {
        // 完美判定时减速效果更强（动能传递更充分）
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