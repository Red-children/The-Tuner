using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class UIPanelDialogue : UIBasePanel
{
    //  对话数据
    [Header("对话UI脚本")]
    public UICommunication uiCommunication;
    [Header("归属的NPC")]
    public NPCCommunication currentNPC;
    //  面板动画
    [Header("动画组件")]
    [Header("动画参数")]
    // 动画配置（可复用，改数字就行）
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float rotateDuration = 0.5f;
    [SerializeField] private float scaleDuration = 0.5f;
    private Sequence _seq;
    [Header("底层图片")]
    [SerializeField] private Image[] background;
    [Header("底层环")]
    [SerializeField] private Image bgRing;
    [Header("红色文本框")]
    // [SerializeField] private Transform transMidRedBox;
    [SerializeField] private Image[] imagesMidRedBox;
    [Header("中层环")]
    [SerializeField] private Transform midRing;
    [Header("黄色文本框")]
    // [SerializeField] private Transform transForeYellowBox;
    [SerializeField] private Image[] imagesForeYellowBox;
    [SerializeField] private Image[] halosForeYellowBox;
    [Header("装饰层")]
    [SerializeField] private Image haloMask;
    [SerializeField] private Image blackMask;

#region 覆写动画
    protected override void PlayEnterAnimation()
    {
        _isPlayingAnimation = true;

        _seq.Join(EnterBackground());
        _seq.Join(EnterBackgroundRing());
        _seq.Join(EnterMidRing());
        _seq.Join(EnterMidRedBox());
        _seq.Join(EnterForeYellowBox());
        _seq.AppendCallback(IdleHaloMask); // 循环动画用Callback

        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
        });

        _seq.SetTarget(gameObject);
    }

    void KillAllLoopingAnimations()
    {
        if(_seq == null) return;
        _seq.Kill();

        if (bgRing) bgRing.rectTransform.DOKill();
        if (midRing) midRing.DOKill();
        if (haloMask) { haloMask.rectTransform.DOKill(); haloMask.DOKill(); }
    }
    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;
        KillAllLoopingAnimations();

        _seq = DOTween.Sequence();
        _seq.Join(ExitBlackMask());
        _seq.Join(ExitHaloMask());
        _seq.Join(ExitForeYellowBox());
        _seq.Join(ExitMidRedBox());
        _seq.Join(ExitMidRing());
        _seq.Join(ExitBackgroundRing());
        _seq.Join(ExitBackground());

        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            OnCloseComplete?.Invoke();
            if(destroyAfter)
                Destroy(gameObject);
        });

        _seq.SetTarget(gameObject);
    }
#endregion

#region 生命周期
    private void Awake()
    {
        //  Override Settings
        exitAnimDuration = 1.2f;
        _seq =DOTween.Sequence();
        // texts = new Text[2];
    }
#endregion

#region 过场动画
    Tween EnterBackground()
    {
        return FadeIn(background, fadeDuration);
    }

    Tween EnterBackgroundRing()
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(FadeIn(bgRing, fadeDuration));
        seq.Join(RotateToZero(bgRing.rectTransform, 5f, rotateDuration));
        seq.OnComplete(IdleBackgroundRing);
        return seq;
    }

    Tween EnterMidRing()
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(ScaleIn(midRing, scaleDuration));
        seq.Join(RotateToZero(midRing, 5f, rotateDuration));
        seq.OnComplete(IdleMidRing);
        return seq;
    }

    Tween EnterMidRedBox()
    {
        return FadeInRotateIn(imagesMidRedBox, (float)0.75 * fadeDuration, 5f, rotateDuration);
    }

    Tween EnterForeYellowBox()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(FadeInRotateIn(imagesForeYellowBox, fadeDuration, 5f, rotateDuration));
        seq.Append(ResetAndFillFadeIn(halosForeYellowBox, (float)0.25 * fadeDuration));
        return seq;
    }
#endregion

#region 驻场动画
    void IdleBackgroundRing()
    {
        if (bgRing == null) return;
        bgRing.rectTransform
              .DORotate(new Vector3(0, 0, -360), 180f, RotateMode.FastBeyond360)
              .SetLoops(-1)
              .SetEase(Ease.Linear)
              .SetUpdate(true);
    }
    void IdleMidRing()
    {
        if (midRing == null) return;
        midRing.DORotate(new Vector3(0, 0, -360), 180f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetUpdate(true);
    }
    void IdleHaloMask()
    {
        if (haloMask == null) return;
        haloMask.rectTransform.DOLocalMoveX(16f, 3f)
                .SetRelative(true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

        haloMask.rectTransform.DOLocalMoveY(16f, 3.6f)
                .SetRelative(true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        haloMask.DOFade(1f,3.6f).From(0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
    }
#endregion

#region 退场动画
    Tween ExitBackground()
    {
        return FadeOut(background, fadeDuration);
    }

    Tween ExitBackgroundRing()
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(FadeOut(bgRing, fadeDuration));
        seq.Join(RotateFromZero(bgRing.rectTransform, 5f, rotateDuration));
        return seq;
    }

    Tween ExitMidRing()
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(ScaleOut(midRing, scaleDuration));
        seq.Join(RotateFromZero(midRing, 5f, rotateDuration));
        return seq;
    }

    Tween ExitMidRedBox()
    {
        return FadeOutRotateOut(imagesMidRedBox, (float)0.75 * fadeDuration, 5f, rotateDuration);
    }

    Tween ExitForeYellowBox()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(FadeOutFillOut(halosForeYellowBox, (float)0.25 * fadeDuration));
        seq.Append(FadeOutRotateOut(imagesForeYellowBox, (float)0.75 * fadeDuration, 5f, rotateDuration));
        return seq;
    }

    Tween ExitHaloMask()
    {
        return FadeOut(haloMask, fadeDuration);
    }

    Tween ExitBlackMask()
    {
        return FadeOut(blackMask, (float)0.5 * fadeDuration);
    }
#endregion

#region 业务
    /// 绑定发起对话的NPC
    public void BindNPC(NPCCommunication npc)
    {
        currentNPC = npc;
    }
    /// 显示对话UI
    public void ShowDialogue(string[] lines)
    {
        gameObject.SetActive(true);
        uiCommunication.StartDialogue(lines);
    }

    /// 隐藏对话UI（对话结束时调用）
    public void HideDialogue()
    {
        gameObject.SetActive(false);
        // 通知NPC结束对话
        currentNPC?.EndDialogue();
        DialogueManager.Instance.EndDialogue();
    }
#endregion
}