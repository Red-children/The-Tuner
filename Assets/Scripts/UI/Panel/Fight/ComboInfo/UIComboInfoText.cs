using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class UIComboInfoText : MonoBehaviour
{
    public TextMeshProUGUI text;
    
    [Header("DOTween 动画参数")]
    public float scaleDuration = 0.2f;
    public float punchScale = 1.5f;
    
    // 保存原始位置和缩放，防止位置累积
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Tween _currentScaleTween;
    private Tween _currentPositionTween;
    
    void Init()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }
        
        if (text == null)
        {
            Debug.LogError("UIComboInfoText 未找到 TextMeshProUGUI 组件");
            return;
        }
        
        // 保存原始位置和缩放
        _originalPosition = text.transform.localPosition;
        _originalScale = text.transform.localScale;
    }
    
    #region 生命周期
    void Start()
    {
        Init();
    }
    
    void OnDestroy()
    {
        _currentScaleTween?.Kill();
        _currentPositionTween?.Kill();
    }
    #endregion

    #region 对外接口
    public void SetDisplayText(string buf)
    {
        text.text = buf;
        Debug.Log($"UIComboInfoText Set Display Text {buf}");
    }
    
    /// <summary>
    /// 使用 DOTween 播放文本动画（仅缩放，不改颜色）
    /// </summary>
    public void TextAnimation(RhythmRank rank)
    {
        // 停止当前正在播放的动画
        _currentScaleTween?.Kill();
        _currentPositionTween?.Kill();
        
        // 重置位置和缩放到原始值（防止累积）
        text.transform.localPosition = _originalPosition;
        text.transform.localScale = _originalScale;
        
        // 缩放动画 - 使用 Punch 效果（弹性缩放）
        _currentScaleTween = text.transform
            .DOPunchScale(Vector3.one * punchScale, scaleDuration, vibrato: 1, elasticity: 1)
            .SetEase(Ease.OutBack);
        
        Debug.Log($"UIComboInfoText Play Animation {rank}");
    }
    
    /// <summary>
    /// 数字跳动效果（用于连击数增加时的反馈）
    /// 只做缩放，不改变位置
    /// </summary>
    public void PlayNumberJumpAnimation()
    {
        _currentScaleTween?.Kill();
        
        // 重置缩放
        text.transform.localScale = _originalScale;
        
        // 只做缩放弹跳，不改变位置
        _currentScaleTween = text.transform
            .DOPunchScale(Vector3.one * 0.5f, 0.15f, vibrato: 1, elasticity: 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => 
            {
                // 确保动画结束后缩放恢复
                text.transform.localScale = _originalScale;
            });
    }
    
    /// <summary>
    /// 震动效果（会自动复位）
    /// </summary>
    public void PlayShakeAnimation(float strength = 5f, float duration = 0.2f)
    {
        _currentPositionTween?.Kill();
        
        // 先复位位置
        text.transform.localPosition = _originalPosition;
        
        // 播放震动，动画结束后自动复位
        _currentPositionTween = text.transform
            .DOShakePosition(duration, strength, vibrato: 10, randomness: 90, snapping: false, fadeOut: true)
            .OnComplete(() => 
            {
                text.transform.localPosition = _originalPosition;
            });
    }
    
    /// <summary>
    /// 手动重置位置（如果发生异常时可以调用）
    /// </summary>
    public void ResetPosition()
    {
        text.transform.localPosition = _originalPosition;
        text.transform.localScale = _originalScale;
    }
    #endregion
}