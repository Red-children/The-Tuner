using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class CrosshairAnimator : MonoBehaviour
{
    [System.Serializable]
    struct Crosshair
    {
        public CanvasGroup canvasGroup;
        public Image center;
        public Image inner;
        public Image outer;
        public Image curve;
        public Image halo;
        public Image cross;
    }
    [Header("准星设置")]
    [SerializeField] private Crosshair blue;
    [SerializeField] private Crosshair red;
    [Header("透明度动画速度")]
    [SerializeField] private float fadeToRedSpeed = 0.2f;   // 命中渐变慢
    [SerializeField] private float fadeToBlueSpeed = 0.1f;   // 复原更快

    private Tween _currentTween;

    private void Start()
    {
    }

#region 精准命中 (DOTween 动画)
public void PlayHitAnimation(RhythmRank rank)
    {
        if (rank == RhythmRank.Miss) return;
        Debug.Log("Crosshair PlayHitAnimation");
        // 打断上一个动画，防止重叠
        _currentTween?.Kill();
        _currentTween = null;

        // 创建序列动画
        Sequence seq = DOTween.Sequence();

        // 蓝色淡出 + 红色淡入（同时）
        seq.Join(FadeCrosshair(blue, 0, fadeToRedSpeed));
        seq.Join(FadeCrosshair(red, 1, fadeToRedSpeed));

        // 停留一下 → 快速切回蓝色
        seq.AppendInterval(0.1f);
        seq.Append(FadeCrosshair(red, 0, fadeToBlueSpeed));
        seq.Append(FadeCrosshair(blue, 1, fadeToBlueSpeed));

        _currentTween = seq;
    }
#endregion
    // 渐变一组准星的透明度
    private Tween FadeCrosshair(Crosshair ch, float targetAlpha, float duration)
    {
        if (ch.canvasGroup == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Join(ch.canvasGroup.DOFade(targetAlpha, duration));

        return seq;
    }
    // 节奏指示器
    private void ScaleIndicator()
    {
        
    }
}