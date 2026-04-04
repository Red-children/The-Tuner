using UnityEngine;

/// <summary>
/// 局部卡肉感组件：挂载在被命中对象上，实现局部时间缩放效果
/// 设计理念：避免全局时间缩放，只影响特定对象的动画和移动
/// </summary>
public class LocalHitStop : MonoBehaviour
{
    [Header("卡肉感设置")]
    [SerializeField] private float defaultHitStopDuration = 0.08f;
    [SerializeField] private float minTimeScale = 0.1f;
    [SerializeField] private AnimationCurve timeScaleCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1f);
    
    [Header("影响组件")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private MonoBehaviour[] affectedComponents; // 受影响的移动/攻击组件
    
    private float _currentTimeScale = 1f;
    private float _hitStopTimer = 0f;
    private float _hitStopDuration = 0f;
    private bool _isInHitStop = false;
    
    private float[] _originalSpeeds; // 保存原始速度
    
    private void Awake()
    {
        // 自动获取动画器
        if (targetAnimator == null)
            targetAnimator = GetComponent<Animator>();
            
        // 修复：正确计算数组大小（动画器 + 其他组件）
        int arraySize = (targetAnimator != null ? 1 : 0) + (affectedComponents != null ? affectedComponents.Length : 0);
        _originalSpeeds = new float[arraySize];
        
        Debug.Log($"[{name}] LocalHitStop初始化完成，数组大小: {arraySize}");
    }
    
    private void Update()
    {
        if (!_isInHitStop) return;
        
        // 更新卡肉感计时
        _hitStopTimer -= Time.unscaledDeltaTime;
        
        if (_hitStopTimer <= 0f)
        {
            EndHitStop();
            return;
        }
        
        // 根据进度计算当前时间缩放
        float progress = 1f - (_hitStopTimer / _hitStopDuration);
        _currentTimeScale = timeScaleCurve.Evaluate(progress);
        
        // 应用局部时间缩放
        ApplyLocalTimeScale(_currentTimeScale);
    }
    
    /// <summary>
    /// 触发卡肉感效果
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    /// <param name="intensity">强度（0-1）</param>
    public void TriggerHitStop(float duration = -1f, float intensity = 1f)
    {
        if (_isInHitStop) return; // 避免叠加
        
        _hitStopDuration = duration > 0 ? duration : defaultHitStopDuration * intensity;
        _hitStopTimer = _hitStopDuration;
        _isInHitStop = true;
        
        // 保存原始速度
        SaveOriginalSpeeds();
        
        Debug.Log($"[{name}] 触发卡肉感: {_hitStopDuration:F3}s, 强度: {intensity:F2}");
    }
    
    /// <summary>
    /// 根据节奏判定等级触发卡肉感
    /// </summary>
    public void TriggerHitStopByRank(RhythmRank rank)
    {
        float intensity = GetIntensityByRank(rank);
        float duration = GetDurationByRank(rank);
        
        TriggerHitStop(duration, intensity);
    }
    
    private void SaveOriginalSpeeds()
    {
        int index = 0;
        
        // 保存动画器原始速度
        if (targetAnimator != null)
        {
            _originalSpeeds[index] = targetAnimator.speed;
            index++;
        }
            
        // 保存其他组件原始速度（如果有的话）
        if (affectedComponents != null)
        {
            for (int i = 0; i < affectedComponents.Length; i++)
            {
                if (affectedComponents[i] != null && index < _originalSpeeds.Length)
                {
                    // 这里可以根据具体组件类型保存相应的速度值
                    // 例如：移动组件的移动速度、攻击组件的攻击速度等
                    _originalSpeeds[index] = 1f; // 默认速度值
                    index++;
                }
            }
        }
        
        Debug.Log($"[{name}] 保存了 {index} 个组件的原始速度");
    }
    
    private void ApplyLocalTimeScale(float timeScale)
    {
        int index = 0;
        
        // 应用动画器速度
        if (targetAnimator != null)
        {
            targetAnimator.speed = timeScale;
            index++;
        }
            
        // 应用其他组件速度（如果有的话）
        if (affectedComponents != null)
        {
            for (int i = 0; i < affectedComponents.Length; i++)
            {
                if (affectedComponents[i] != null && index < _originalSpeeds.Length)
                {
                    // 这里可以根据具体组件类型应用时间缩放
                    // 例如：移动组件的移动速度、攻击组件的攻击速度等
                    index++;
                }
            }
        }
    }
    
    private void EndHitStop()
    {
        _isInHitStop = false;
        _currentTimeScale = 1f;
        
        int index = 0;
        
        // 恢复原始速度
        if (targetAnimator != null && index < _originalSpeeds.Length)
        {
            targetAnimator.speed = _originalSpeeds[index];
            index++;
        }
            
        // 恢复其他组件速度
        if (affectedComponents != null)
        {
            for (int i = 0; i < affectedComponents.Length; i++)
            {
                if (affectedComponents[i] != null && index < _originalSpeeds.Length)
                {
                    // 恢复组件原始速度
                    index++;
                }
            }
        }
        
        Debug.Log($"[{name}] 卡肉感结束，恢复了 {index} 个组件的速度");
    }
    
    private float GetIntensityByRank(RhythmRank rank)
    {
        return rank switch
        {
            RhythmRank.Perfect => 1.0f,
            RhythmRank.Great => 0.7f,
            RhythmRank.Good => 0.4f,
            _ => 0.2f // Miss或其他情况
        };
    }
    
    private float GetDurationByRank(RhythmRank rank)
    {
        return rank switch
        {
            RhythmRank.Perfect => 0.1f,
            RhythmRank.Great => 0.07f,
            RhythmRank.Good => 0.04f,
            _ => 0.02f // Miss或其他情况
        };
    }
    
    /// <summary>
    /// 获取当前是否处于卡肉感状态
    /// </summary>
    public bool IsInHitStop => _isInHitStop;
    
    /// <summary>
    /// 获取当前时间缩放值
    /// </summary>
    public float CurrentTimeScale => _currentTimeScale;
}