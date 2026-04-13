using UnityEngine;
using System.Collections;

/// <summary>
/// 子弹曲线控制减速组件：使用动画曲线精确控制减速过程
/// 设计理念：通过曲线实现复杂的减速模式，支持多段减速和精确调校
/// 机械工程类比：精密机械的减速器 - 多级变速，精确控制
/// </summary>
public class BulletCurveSlowdown : MonoBehaviour
{
    [Header("曲线减速设置")]
    // 基础减速曲线（默认使用EaseInOut）
    [SerializeField] public AnimationCurve slowdownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.2f);
    // 恢复曲线（默认使用EaseInOut）
    [SerializeField] public AnimationCurve recoveryCurve = AnimationCurve.EaseInOut(0, 0.2f, 1, 1f);
    
    [Header("多段减速模式")]
    [SerializeField] private bool enableMultiStage = true;

    [SerializeField] private SlowdownStage[] slowdownStages = new SlowdownStage[]
    {
        new SlowdownStage(0.1f, 0.3f, "初始快速减速"),
        new SlowdownStage(0.3f, 0.6f, "持续缓慢减速"),
        new SlowdownStage(0.2f, 0.8f, "缓慢恢复阶段")
    };
    
    [Header("节奏判定影响")]
    // 根据节奏判定等级调整曲线参数 
    [SerializeField] private AnimationCurve perfectCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.1f);
    [SerializeField] private AnimationCurve greatCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.2f);
    [SerializeField] private AnimationCurve goodCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.3f);
    [SerializeField] private AnimationCurve missCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.5f);
    
    [Header("视觉曲线效果")]
    // 视觉效果曲线（尺寸、旋转、透明度等）
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.7f);
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 360f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.8f);
    
    [Header("高级设置")]
    [SerializeField] private bool useCustomCurveForRank = true;
    [SerializeField] private float curveBlendSpeed = 2f; // 曲线切换速度
    
    private Bullet _bullet;             //子弹组件引用
    private float _originalSpeed;       //原始速度
    private Vector3 _originalScale;     //原始尺寸
    private SpriteRenderer _spriteRenderer; //用于视觉效果的SpriteRenderer
    private Color _originalColor;           //原始颜色

    private Coroutine _slowdownCoroutine;   //当前减速协程引用
    private AnimationCurve _currentCurve;   //当前使用的曲线（支持动态切换）
    private RhythmRank _currentRank;        //当前节奏判定等级

    private void Awake()
    {
        _bullet = GetComponent<Bullet>();   // 获取Bullet组件
        if (_bullet == null)
        {
            Debug.LogWarning($"[{name}] 未找到Bullet组件，曲线减速效果将无法工作");
            return;
        }
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
        
        _originalSpeed = _bullet.moveSpeed;
        _originalScale = transform.localScale;
        _currentCurve = slowdownCurve;
        
        Debug.Log($"[{name}] 曲线减速组件初始化完成");
    }
    
    /// <summary>
    /// 触发曲线减速效果
    /// </summary>
    public void TriggerCurveSlowdown(RhythmRank rank = RhythmRank.Good)
    {
        if (_bullet == null) return;
        
        // 停止之前的减速协程
        if (_slowdownCoroutine != null)
        {
            StopCoroutine(_slowdownCoroutine);
        }
        
        _currentRank = rank;
        
        // 根据模式选择减速方式
        if (enableMultiStage)
        {
            _slowdownCoroutine = StartCoroutine(MultiStageSlowdown());
        }
        else
        {
            _slowdownCoroutine = StartCoroutine(SingleCurveSlowdown());
        }
        
        Debug.Log($"[{name}] 曲线减速触发: {rank}");
    }
    
    /// <summary>
    /// 单曲线减速模式
    /// </summary>
    private IEnumerator SingleCurveSlowdown()
    {
        float duration = GetDurationByRank(_currentRank);
        float timer = 0f;
        
        // 获取对应的曲线
        AnimationCurve curve = GetCurveByRank(_currentRank);
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            
            // 应用曲线减速
            float speedMultiplier = curve.Evaluate(progress); //根据对应曲线的进度求函数值
            ApplyCurveEffects(progress, speedMultiplier);//每帧应用曲线效果

            yield return null;
        }
        
        // 恢复原始状态
        ApplyCurveEffects(1f, 1f);
    }
    
    /// <summary>
    /// 多段减速模式（精密控制）
    /// </summary>
    private IEnumerator MultiStageSlowdown()
    {
        float totalDuration = 0f;
        
        // 计算总持续时间
        foreach (var stage in slowdownStages)
        {
            totalDuration += stage.duration;
        }
        
        float accumulatedTime = 0f;
        
        // 执行每一段减速
        for (int i = 0; i < slowdownStages.Length; i++)
        {
            var stage = slowdownStages[i];
            float stageTimer = 0f;
            
            Debug.Log($"[{name}] 开始减速阶段 {i + 1}: {stage.description}");
            
            while (stageTimer < stage.duration)
            {
                stageTimer += Time.deltaTime;
                accumulatedTime += Time.deltaTime;
                
                float stageProgress = stageTimer / stage.duration;
                float totalProgress = accumulatedTime / totalDuration;
                
                // 计算当前阶段的速度乘数
                float stageSpeed = Mathf.Lerp(stage.startSpeed, stage.endSpeed, stageProgress);
                
                // 应用多段效果
                ApplyMultiStageEffects(totalProgress, stageSpeed, i);
                
                yield return null;
            }
        }
        
        // 最终恢复
        ApplyCurveEffects(1f, 1f);
    }
    
    /// <summary>
    /// 应用曲线减速效果
    /// </summary>
    private void ApplyCurveEffects(float progress, float speedMultiplier)
    {
        // 应用速度减速
        _bullet.moveSpeed = (int)(_originalSpeed * speedMultiplier);
        
        // 应用视觉曲线效果
        if (_spriteRenderer != null)
        {
            // 尺寸变化
            float scale = scaleCurve.Evaluate(progress);
            transform.localScale = _originalScale * scale;
            
            // 旋转效果
            float rotation = rotationCurve.Evaluate(progress);
            transform.rotation = Quaternion.Euler(0, 0, rotation);
            
            // 透明度变化
            float alpha = alphaCurve.Evaluate(progress);
            Color currentColor = _spriteRenderer.color;
            currentColor.a = alpha;
            _spriteRenderer.color = currentColor;
        }
    }
    
    /// <summary>
    /// 应用多段减速效果
    /// </summary>
    private void ApplyMultiStageEffects(float totalProgress, float speedMultiplier, int stageIndex)
    {
        // 应用速度减速
        _bullet.moveSpeed = (int)(_originalSpeed * speedMultiplier);
        
        // 根据阶段应用不同的视觉效果
        switch (stageIndex)
        {
            case 0: // 第一阶段：快速减速
                ApplyFastSlowdownEffects(totalProgress);
                break;
            case 1: // 第二阶段：持续减速
                ApplySustainedEffects(totalProgress);
                break;
            case 2: // 第三阶段：恢复阶段
                ApplyRecoveryEffects(totalProgress);
                break;
        }
    }
    
    /// <summary>
    /// 快速减速阶段的视觉效果
    /// </summary>
    private void ApplyFastSlowdownEffects(float progress)
    {
        if (_spriteRenderer != null)
        {
            // 快速缩放效果
            float scale = Mathf.Lerp(1f, 0.7f, progress);
            transform.localScale = _originalScale * scale;
            
            // 颜色变红（撞击效果）
            Color impactColor = Color.Lerp(_originalColor, Color.red, progress);
            _spriteRenderer.color = impactColor;
        }
    }
    
    /// <summary>
    /// 持续减速阶段的视觉效果
    /// </summary>
    private void ApplySustainedEffects(float progress)
    {
        if (_spriteRenderer != null)
        {
            // 轻微震动效果
            float shake = Mathf.Sin(progress * 20f) * 0.1f;
            transform.localPosition += new Vector3(shake, 0, 0);
            
            // 颜色渐变
            Color sustainedColor = Color.Lerp(Color.red, Color.yellow, progress);
            _spriteRenderer.color = sustainedColor;
        }
    }
    
    /// <summary>
    /// 恢复阶段的视觉效果
    /// </summary>
    private void ApplyRecoveryEffects(float progress)
    {
        if (_spriteRenderer != null)
        {
            // 平滑恢复尺寸
            float scale = Mathf.Lerp(0.7f, 1f, progress);
            transform.localScale = _originalScale * scale;
            
            // 颜色恢复
            Color recoveryColor = Color.Lerp(Color.yellow, _originalColor, progress);
            _spriteRenderer.color = recoveryColor;
        }
    }
    
    /// <summary>
    /// 根据节奏判定等级获取对应的曲线
    /// </summary>
    private AnimationCurve GetCurveByRank(RhythmRank rank)
    {
        if (!useCustomCurveForRank) return slowdownCurve;
        
        return rank switch
        {
            RhythmRank.Perfect => perfectCurve,
            RhythmRank.Great => greatCurve,
            RhythmRank.Good => goodCurve,
            RhythmRank.Miss => missCurve,
            _ => slowdownCurve
        };
    }
    
    /// <summary>
    /// 根据节奏判定等级获取持续时间
    /// </summary>
    private float GetDurationByRank(RhythmRank rank)
    {
        return rank switch
        {
            RhythmRank.Perfect => 0.2f,  // 完美：最长持续时间
            RhythmRank.Great => 0.15f,   // 优秀：中等持续时间
            RhythmRank.Good => 0.1f,     // 良好：较短持续时间
            RhythmRank.Miss => 0.05f,    // 失误：最短持续时间
            _ => 0.1f
        };
    }
    
    /// <summary>
    /// 动态切换曲线（用于实时调整）
    /// </summary>
    public void SwitchToCurve(AnimationCurve newCurve, float blendTime = 0.5f)
    {
        StartCoroutine(BlendCurves(newCurve, blendTime));
    }
    
    /// <summary>
    /// 曲线混合过渡
    /// </summary>
    private IEnumerator BlendCurves(AnimationCurve targetCurve, float blendTime)
    {
        AnimationCurve startCurve = _currentCurve;
        float timer = 0f;
        
        while (timer < blendTime)
        {
            timer += Time.deltaTime; //增加计时器
            float blendFactor = timer / blendTime;// 逐渐混合曲线参数，实现平滑过渡
            
            // 创建混合曲线（简化实现）
            _currentCurve = CreateBlendedCurve(startCurve, targetCurve, blendFactor);
            
            yield return null;
        }
        
        // 最终切换到目标曲线
        _currentCurve = targetCurve;
    }
    
    /// <summary>
    /// 创建混合曲线（简化实现）
    /// </summary>
    private AnimationCurve CreateBlendedCurve(AnimationCurve curveA, AnimationCurve curveB, float blendFactor)
    {
        // 简化实现：在实际项目中可能需要更复杂的混合算法
        Keyframe[] blendedKeys = new Keyframe[curveA.length];
        
        for (int i = 0; i < curveA.length; i++)
        {
            float time = curveA[i].time;            //假设两条曲线的关键帧时间相同，实际项目中可能需要更复杂的处理

            float valueA = curveA.Evaluate(time);   //根据时间求函数值

            float valueB = curveB.Evaluate(time);

            float blendedValue = Mathf.Lerp(valueA, valueB, blendFactor);//线性插值计算混合值，实现平滑过渡
            
            blendedKeys[i] = new Keyframe(time, blendedValue);
        }
        
        return new AnimationCurve(blendedKeys);
    }
    
    /// <summary>
    /// 获取当前是否处于减速状态
    /// </summary>
    public bool IsSlowingDown => _slowdownCoroutine != null;
    
    /// <summary>
    /// 停止当前减速效果
    /// </summary>
    public void StopSlowdown()
    {
        if (_slowdownCoroutine != null)
        {
            StopCoroutine(_slowdownCoroutine);
            _slowdownCoroutine = null;
            ApplyCurveEffects(1f, 1f); // 立即恢复
        }
    }
}

/// <summary>
/// 减速阶段数据结构
/// </summary>
[System.Serializable]
public struct SlowdownStage
{
    public float duration;      // 阶段持续时间
    public float startSpeed;    // 阶段开始速度乘数
    public float endSpeed;      // 阶段结束速度乘数
    public string description;  // 阶段描述
    
    public SlowdownStage(float dur, float endSpd, string desc = "")
    {
        duration = dur;
        startSpeed = 1f; // 默认从1开始
        endSpeed = endSpd;
        description = desc;
    }
}