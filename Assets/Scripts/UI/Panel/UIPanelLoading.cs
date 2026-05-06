using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelLoading : UIBasePanel
{
    [Header("动画参数")]
    [SerializeField] private float rotateDuration = 0.4f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float scaleDuration = 0.3f;
    [Header("动画组件")]
    [Header("Background")]
    [SerializeField] private Image backgroundColor;
    [SerializeField] private Image backgroundDecoration;
    [SerializeField] private Image backgroundBottom;
    [Header("Mid")]
    [SerializeField] private Image midRibbon;
    [SerializeField] private Image midCube;
    [SerializeField] private Image midCircle;
    [Header("Foreground")]
    [SerializeField] private Image foregroundCircle;
    [SerializeField] private Image loadingBackground;
    [SerializeField] private Image loadingBottom;
    [SerializeField] private Image loadingMask;
    [Header("Decoration")]
    [SerializeField] private Image decCircle;
    [SerializeField] private Image decCubes;
    [Header("Screen")]
    [SerializeField] private Image screenMask;
    [SerializeField] private CanvasGroup canvasGroup;
    #region 覆写动画
    protected override void PlayEnterAnimation()
    {
        if (_seq != null)
        {
            _seq.Kill();
            _seq = null;
        }
        _seq = DOTween.Sequence();

        _isPlayingAnimation = true;
        //  TODO: Add Tweens here.
        _seq.Join(EnterBackground());
        _seq.Join(EnterMidCircle());
        _seq.Append(EnterMidRibbon());
        _seq.Join(EnterMidCube());
        _seq.Append(EnterForegroundCircle());
        _seq.Join(EnterLoadingLine());
        _seq.Join(EnterDecorations());
        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;

            TriggerOnOpenComplete();
        });
        _seq.SetUpdate(true);
        _seq.SetTarget(gameObject);
    }
    protected override void KillAllLoopingAnimations()
    {
        base.KillAllLoopingAnimations();
    }
    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;
        KillAllLoopingAnimations();

        _seq = DOTween.Sequence();
        _seq.OnStart(() =>
        {
        });
        //  TODO: Add Tweens here.
        _seq.Join(ExitAnimation());
        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            TriggerOnCloseComplete();
            if (destroyAfter)
                Destroy(gameObject);
            else HideImmediately();
        });
        _seq.SetUpdate(true);
        _seq.SetTarget(gameObject);
    }
#endregion
#region 进场动画
    Tween EnterBackground()
    {
        if (backgroundColor == null || backgroundBottom == null || backgroundDecoration == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Join(FadeIn(backgroundColor, 0.5f * fadeDuration));
        seq.Join(FillIn(backgroundDecoration, 0.5f *fadeDuration));
        seq.Join(FillIn(backgroundBottom, 0.5f * fadeDuration));
        return seq;
    }
    Tween EnterMidRibbon()
    {
        if (midRibbon == null) return null;
        return ResetAndFillFadeIn(midRibbon, 0.4f * fadeDuration);
    }
    Tween EnterMidCube()
    {
        if (midCube == null) return null;
        return ScaleIn(midCube.rectTransform, scaleDuration);
    }
    Tween EnterMidCircle()
    {
        if (midCircle == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Join(FadeIn(midCircle, fadeDuration));
        seq.Join(ScaleIn(midCircle.rectTransform, scaleDuration));
        return seq;
    }
    Tween EnterForegroundCircle()
    {
        if (foregroundCircle == null) return null;
        return FadeIn(foregroundCircle, 0.5f * fadeDuration);
    }
    Tween EnterLoadingLine()
    {
        if (loadingBackground == null || loadingBottom == null || loadingMask == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Append(loadingBackground.rectTransform.DOScaleX(1, scaleDuration).From(0));
        seq.Append(ResetAndFillFadeIn(loadingBottom,scaleDuration));
        seq.Join(loadingMask.DOFillAmount(0.35f, scaleDuration));
        return seq;
    }
    Tween EnterDecorations()
    {
        if (decCircle == null || decCubes == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Join(FadeIn(decCircle, fadeDuration));
        seq.Join(FadeIn(decCubes, fadeDuration));
        return seq;
    }
#endregion
#region 退场动画
    Tween ExitAnimation()
    {
        if (screenMask == null) return null;
        if (canvasGroup == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Append(FadeIn(screenMask, fadeDuration));
        seq.Append(Fadeout(canvasGroup, fadeDuration));
        return seq;
    }
#endregion
#region 结束信号
    public void Complete(bool delete)
    {
        if (loadingMask == null) return;
        
        loadingMask.DOFillAmount(1f, scaleDuration)
                   .OnComplete(() =>
                   {
                       if (delete) UIManager.Instance.ClosePanel(this);
                       else HidePanel();
                   });
    }
#endregion
}
