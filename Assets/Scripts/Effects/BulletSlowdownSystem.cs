using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 子弹减速系统：组件化设计，支持灵活的减速效果组合
/// 设计理念：模块化架构，每个减速组件独立工作，易于扩展和维护
/// 机械工程类比：模块化生产线 - 不同工站独立工作，灵活组合
/// </summary>
public class BulletSlowdownSystem : MonoBehaviour
{
    [Header("系统配置")]
    [SerializeField] private bool enableSystem = true;
    [SerializeField] private SystemMode systemMode = SystemMode.Parallel;
    
    [Header("减速组件列表")]
    [SerializeField] private List<SlowdownComponent> slowdownComponents = new List<SlowdownComponent>();
    
    [Header("组件配置")]
    [SerializeField] private SlowdownConfig[] componentConfigs;
    
    [Header("事件系统")]
    [SerializeField] private bool enableEventSystem = true;
    [SerializeField] private ComponentEvent[] componentEvents;
    
    private Bullet _bullet;
    private float _originalSpeed;
    private Dictionary<SlowdownComponent, Coroutine> _activeCoroutines = new Dictionary<SlowdownComponent, Coroutine>();
    private List<SlowdownComponent> _activeComponents = new List<SlowdownComponent>();
    
    private void Awake()
    {
        _bullet = GetComponent<Bullet>();
        if (_bullet == null)
        {
            Debug.LogWarning($"[{name}] 未找到Bullet组件，减速系统将无法工作");
            return;
        }
        
        _originalSpeed = _bullet.moveSpeed;
        
        // 初始化所有组件
        InitializeComponents();
        
        Debug.Log($"[{name}] 子弹减速系统初始化完成，组件数量: {slowdownComponents.Count}");
    }
    
    /// <summary>
    /// 初始化所有减速组件
    /// </summary>
    private void InitializeComponents()
    {
        foreach (var component in slowdownComponents)
        {
            if (component != null)
            {
                component.Initialize(_bullet, _originalSpeed);
            }
        }
    }
    
    /// <summary>
    /// 触发组件化减速效果
    /// </summary>
    public void TriggerComponentSlowdown(RhythmRank rank, Collision2D collision = null)
    {
        if (!enableSystem || _bullet == null) return;
        
        // 停止所有正在运行的组件
        StopAllComponents();
        
        // 根据系统模式执行减速
        switch (systemMode)
        {
            case SystemMode.Parallel:
                StartCoroutine(ExecuteParallelSlowdown(rank, collision));
                break;
            case SystemMode.Sequential:
                StartCoroutine(ExecuteSequentialSlowdown(rank, collision));
                break;
            case SystemMode.Conditional:
                StartCoroutine(ExecuteConditionalSlowdown(rank, collision));
                break;
        }
        
        Debug.Log($"[{name}] 组件减速系统触发: {rank}, 模式: {systemMode}");
    }
    
    /// <summary>
    /// 并行执行模式：所有组件同时运行
    /// </summary>
    private IEnumerator ExecuteParallelSlowdown(RhythmRank rank, Collision2D collision)
    {
        _activeComponents.Clear();
        
        // 并行启动所有组件
        foreach (var component in slowdownComponents)
        {
            if (component != null && component.IsEnabled)
            {
                var coroutine = StartCoroutine(component.ExecuteSlowdown(rank, collision));
                _activeCoroutines[component] = coroutine;
                _activeComponents.Add(component);
            }
        }
        
        // 等待所有组件完成
        yield return WaitForAllComponents();
        
        // 清理完成后的组件
        CleanupCompletedComponents();
    }
    
    /// <summary>
    /// 顺序执行模式：组件按顺序运行
    /// </summary>
    private IEnumerator ExecuteSequentialSlowdown(RhythmRank rank, Collision2D collision)
    {
        foreach (var component in slowdownComponents)
        {
            if (component != null && component.IsEnabled)
            {
                yield return component.ExecuteSlowdown(rank, collision);
            }
        }
    }
    
    /// <summary>
    /// 条件执行模式：根据条件选择组件
    /// </summary>
    private IEnumerator ExecuteConditionalSlowdown(RhythmRank rank, Collision2D collision)
    {
        // 根据节奏判定选择组件
        var selectedComponents = SelectComponentsByRank(rank);
        
        foreach (var component in selectedComponents)
        {
            if (component != null && component.IsEnabled)
            {
                yield return component.ExecuteSlowdown(rank, collision);
            }
        }
    }
    
    /// <summary>
    /// 根据节奏判定等级选择组件
    /// </summary>
    private List<SlowdownComponent> SelectComponentsByRank(RhythmRank rank)
    {
        var selected = new List<SlowdownComponent>();
        
        foreach (var component in slowdownComponents)
        {
            if (component != null && component.IsEnabled)
            {
                // 根据组件的优先级和节奏判定选择
                if (component.ShouldExecuteForRank(rank))
                {
                    selected.Add(component);
                }
            }
        }
        
        // 按优先级排序
        selected.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        
        return selected;
    }
    
    /// <summary>
    /// 等待所有活动组件完成
    /// </summary>
    private IEnumerator WaitForAllComponents()
    {
        bool allCompleted = false;
        
        while (!allCompleted)
        {
            allCompleted = true;
            
            foreach (var component in _activeComponents)
            {
                if (component != null && component.IsExecuting)
                {
                    allCompleted = false;
                    break;
                }
            }
            
            if (!allCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    /// <summary>
    /// 清理已完成组件
    /// </summary>
    private void CleanupCompletedComponents()
    {
        var completedComponents = new List<SlowdownComponent>();
        
        foreach (var component in _activeComponents)
        {
            if (component != null && !component.IsExecuting)
            {
                completedComponents.Add(component);
                
                if (_activeCoroutines.ContainsKey(component))
                {
                    _activeCoroutines.Remove(component);
                }
            }
        }
        
        foreach (var completed in completedComponents)
        {
            _activeComponents.Remove(completed);
        }
    }
    
    /// <summary>
    /// 停止所有组件
    /// </summary>
    public void StopAllComponents()
    {
        // 停止所有协程
        foreach (var coroutine in _activeCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        
        _activeCoroutines.Clear();
        
        // 停止所有组件
        foreach (var component in slowdownComponents)
        {
            if (component != null)
            {
                component.StopExecution();
            }
        }
        
        _activeComponents.Clear();
        
        // 恢复原始速度
        _bullet.moveSpeed = (int)_originalSpeed;
    }
    
    /// <summary>
    /// 添加新的减速组件
    /// </summary>
    public void AddComponent(SlowdownComponent component)
    {
        if (component != null && !slowdownComponents.Contains(component))
        {
            slowdownComponents.Add(component);
            component.Initialize(_bullet, _originalSpeed);
        }
    }
    
    /// <summary>
    /// 移除减速组件
    /// </summary>
    public void RemoveComponent(SlowdownComponent component)
    {
        if (component != null && slowdownComponents.Contains(component))
        {
            slowdownComponents.Remove(component);
            component.StopExecution();
        }
    }
    
    /// <summary>
    /// 获取系统状态
    /// </summary>
    public SystemStatus GetSystemStatus()
    {
        int activeCount = 0;
        int totalCount = slowdownComponents.Count;
        
        foreach (var component in slowdownComponents)
        {
            if (component != null && component.IsExecuting)
            {
                activeCount++;
            }
        }
        
        return new SystemStatus
        {
            isActive = _activeComponents.Count > 0,
            activeComponentCount = activeCount,
            totalComponentCount = totalCount,
            currentMode = systemMode
        };
    }
    
    /// <summary>
    /// 动态切换系统模式
    /// </summary>
    public void SwitchSystemMode(SystemMode newMode)
    {
        if (systemMode != newMode)
        {
            StopAllComponents();
            systemMode = newMode;
            Debug.Log($"[{name}] 系统模式切换为: {newMode}");
        }
    }
    
    /// <summary>
    /// 启用/禁用特定组件
    /// </summary>
    public void SetComponentEnabled(SlowdownComponent component, bool enabled)
    {
        if (component != null)
        {
            component.SetEnabled(enabled);
        }
    }
}

/// <summary>
/// 减速组件基类：所有减速组件的抽象基类
/// </summary>
public abstract class SlowdownComponent : MonoBehaviour
{
    [Header("组件配置")]
    [SerializeField] protected bool isEnabled = true;
    [SerializeField] protected int priority = 1;
    [SerializeField] protected RhythmRank[] supportedRanks = new RhythmRank[] 
    { 
        RhythmRank.Perfect, RhythmRank.Great, RhythmRank.Good, RhythmRank.Miss 
    };
    
    protected Bullet _targetBullet;
    protected float _originalSpeed;
    protected bool _isExecuting = false;
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    public virtual void Initialize(Bullet bullet, float originalSpeed)
    {
        _targetBullet = bullet;
        _originalSpeed = originalSpeed;
    }
    
    /// <summary>
    /// 执行减速效果
    /// </summary>
    public abstract IEnumerator ExecuteSlowdown(RhythmRank rank, Collision2D collision);
    
    /// <summary>
    /// 停止执行
    /// </summary>
    public virtual void StopExecution()
    {
        _isExecuting = false;
    }
    
    /// <summary>
    /// 检查是否应该为指定节奏判定执行
    /// </summary>
    public virtual bool ShouldExecuteForRank(RhythmRank rank)
    {
        return System.Array.Exists(supportedRanks, r => r == rank);
    }
    
    /// <summary>
    /// 启用/禁用组件
    /// </summary>
    public virtual void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }
    
    // 属性访问器
    public bool IsEnabled => isEnabled;
    public bool IsExecuting => _isExecuting;
    public int Priority => priority;
}

/// <summary>
/// 速度减速组件：专门处理速度变化
/// </summary>
public class SpeedReducer : SlowdownComponent
{
    [Header("速度减速设置")]
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.3f);
    [SerializeField] private float duration = 0.2f;
    
    public override IEnumerator ExecuteSlowdown(RhythmRank rank, Collision2D collision)
    {
        if (!isEnabled || _targetBullet == null) yield break;
        
        _isExecuting = true;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            
            // 应用速度曲线
            float speedMultiplier = speedCurve.Evaluate(progress);
            _targetBullet.moveSpeed = (int)(_originalSpeed * speedMultiplier);
            
            yield return null;
        }
        
        // 恢复原始速度
        _targetBullet.moveSpeed = (int)_originalSpeed;
        _isExecuting = false;
    }
}

/// <summary>
/// 视觉效果组件：处理视觉反馈
/// </summary>
public class VisualEffectComponent : SlowdownComponent
{
    [Header("视觉效果设置")]
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.7f);
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 180f);
    [SerializeField] private float effectDuration = 0.3f;
    
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    
    public override void Initialize(Bullet bullet, float originalSpeed)
    {
        base.Initialize(bullet, originalSpeed);
        _originalScale = transform.localScale;
        _originalRotation = transform.rotation;
    }
    
    public override IEnumerator ExecuteSlowdown(RhythmRank rank, Collision2D collision)
    {
        if (!isEnabled) yield break;
        
        _isExecuting = true;
        float timer = 0f;
        
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / effectDuration;
            
            // 应用视觉曲线
            float scale = scaleCurve.Evaluate(progress);
            float rotation = rotationCurve.Evaluate(progress);
            
            transform.localScale = _originalScale * scale;
            transform.rotation = _originalRotation * Quaternion.Euler(0, 0, rotation);
            
            yield return null;
        }
        
        // 恢复原始状态
        transform.localScale = _originalScale;
        transform.rotation = _originalRotation;
        _isExecuting = false;
    }
}

/// <summary>
/// 粒子效果组件：处理粒子系统
/// </summary>
public class ParticleEffectComponent : SlowdownComponent
{
    [Header("粒子效果设置")]
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private float particleDuration = 1f;
    
    public override IEnumerator ExecuteSlowdown(RhythmRank rank, Collision2D collision)
    {
        if (!isEnabled || hitParticles == null) yield break;
        
        _isExecuting = true;
        
        // 播放粒子效果
        hitParticles.Play();
        
        // 等待粒子效果完成
        yield return new WaitForSeconds(particleDuration);
        
        _isExecuting = false;
    }
}

// 枚举定义
public enum SystemMode
{
    Parallel,      // 并行执行：所有组件同时运行
    Sequential,    // 顺序执行：组件按顺序运行
    Conditional    // 条件执行：根据条件选择组件
}

// 数据结构
[System.Serializable]
public struct SlowdownConfig
{
    public string componentName;
    public AnimationCurve intensityCurve;
    public float baseDuration;
}

[System.Serializable]
public struct ComponentEvent
{
    public string eventName;
    public SlowdownComponent targetComponent;
    public RhythmRank triggerRank;
}

public struct SystemStatus
{
    public bool isActive;
    public int activeComponentCount;
    public int totalComponentCount;
    public SystemMode currentMode;
}