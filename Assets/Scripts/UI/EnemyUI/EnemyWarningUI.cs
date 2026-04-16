using DG.Tweening;
using TMPro;
using UnityEngine;

public class EnemyWarningUI : MonoBehaviour
{
    [Header("攻击预警")]
    public TextMeshProUGUI exclamationText;  // קxtMeshPro 组件
    public CanvasGroup canvasGroup;          // 控制UI淡入淡出效果的CanvasGroup组件

    [Header("动画参数")]
    public float fadeInDuration = 0.1f;
    public float holdDuration = 0.3f;        // 持续显示时间
    public float fadeOutDuration = 0.2f;
    public Vector3 punchScale = new Vector3(0.5f, 0.5f, 0.5f);

    private Tween _currentTween;

    private void Awake()
    {
        if (exclamationText == null)
            exclamationText = GetComponentInChildren<TextMeshProUGUI>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 开始设置UI为不可见状态
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 播放攻击预警动画，包含快速淡入、弹出效果、保持显示和快速淡出，最后自动隐藏UI。确保在播放新动画前终止任何正在进行的动画，以避免冲突和视觉混乱。
    /// </summary>
    public void PlayWarning()
{
    gameObject.SetActive(true);
    _currentTween?.Kill();

    canvasGroup.alpha = 0f;
    exclamationText.transform.localScale = Vector3.one * 1.5f; // 起始放大，但不移动

    Sequence seq = DOTween.Sequence();
    
    // 快速淡入 + 缩回正常大小（原地弹出效果）
    seq.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
    seq.Join(exclamationText.transform.DOScale(Vector3.one, fadeInDuration * 2f).SetEase(Ease.OutBack));
    
    // 保持显示
    seq.AppendInterval(holdDuration);
    
    // 快速淡出
    seq.Append(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
    
    seq.OnComplete(() => gameObject.SetActive(false));

    _currentTween = seq;
}

    private void OnDestroy()
    {
        _currentTween?.Kill();
    }
}