using DG.Tweening;
using UnityEngine;

/// <summary>
/// 节拍缩放效果组件
/// 让物体跟随节奏节拍进行呼吸式放大缩小
/// </summary>
public class BeatScaleEffect : MonoBehaviour
{
    [Header("节拍缩放配置")]
    [Tooltip("基础缩放值")]
    public Vector3 baseScale = Vector3.one;
    
    [Tooltip("节拍峰值时的缩放值")]
    public Vector3 beatScale = new Vector3(1.2f, 1.2f, 1.2f);
    
    [Tooltip("缩放动画持续时间（秒）")]
    public float scaleDuration = 0.15f;
    
    [Tooltip("使用的缓动曲线")]
    public Ease easeType = Ease.OutQuad;
    
    [Tooltip("是否在节拍窗口内才触发缩放")]
    public bool onlyOnBeatWindow = false;

    private Tweener _currentTween;
    private Vector3 _targetScale;

    private void Start()
    {
        // 初始化基础缩放
        transform.localScale = baseScale;
        _targetScale = baseScale;
        
        // 订阅节奏数据事件
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
    }

    private void OnDestroy()
    {
        // 取消订阅
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
        
        // 清理动画
        if (_currentTween != null)
            _currentTween.Kill();
    }

    private void OnRhythmData(RhythmData data)
    {
        // 如果设置了只在节拍窗口内触发，且不在窗口内则跳过
        if (onlyOnBeatWindow && !data.isInWindow)
            return;

        // 根据评级设置目标缩放
        if (data.isInWindow)
        {
            // 在节拍窗口内，放大
            _targetScale = baseScale + (beatScale - baseScale) * (float)data.multiplier;
        }
        else
        {
            // 不在节拍窗口，恢复基础缩放
            _targetScale = baseScale;
        }

        // 执行缩放动画
        UpdateScale();
    }

    private void UpdateScale()
    {
        // 杀死当前动画
        if (_currentTween != null && _currentTween.IsActive())
            _currentTween.Kill();

        // 开始新的缩放动画
        _currentTween = transform.DOScale(_targetScale, scaleDuration)
            .SetEase(easeType)
            .SetUpdate(true);
    }

    /// <summary>
    /// 手动触发节拍缩放（可用于调试）
    /// </summary>
    [ContextMenu("Trigger Beat Scale")]
    public void TriggerBeatScale()
    {
        _targetScale = beatScale;
        UpdateScale();
        
        // 延迟恢复
        Invoke(nameof(ResetToBaseScale), scaleDuration * 2);
    }

    private void ResetToBaseScale()
    {
        _targetScale = baseScale;
        UpdateScale();
    }
}