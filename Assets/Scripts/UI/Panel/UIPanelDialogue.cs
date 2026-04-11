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
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float rotateDuration = 1f;
    [SerializeField] private float scaleDuration = 1f;
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

        //  1.底部背景bg淡入
        EnterBackground();
        //  2.底部环bgRing顺时针旋转进场
        EnterBackgroundRing();
        //  3.中间层环midRing旋转放大进场
        EnterMidRing();
        //  4.中间层红色文本框midRedBox进场
        EnterMidRedBox();
        //  5.上层黄色文本框foreYellowBox进场
        EnterForeYellowBox();
        //  6.装饰层常驻浮动特效
        IdleHaloMask();

        DOVirtual.DelayedCall(1.2f, () => _isPlayingAnimation = false);
    }
    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;

        // 先停所有循环动画
        KillAllLoopingAnimations();

        ExitBlackMask();
        ExitHaloMask();
        ExitForeYellowBox();
        ExitMidRedBox();
        ExitMidRing();
        ExitBackgroundRing();
        ExitBackground();

        // 动画结束后关闭面板
        DOVirtual.DelayedCall(exitAnimDuration, () =>
        {
            _isPlayingAnimation = false;

            // 直接销毁游戏对象 → 立刻消失
            Destroy(gameObject);
        });
    }
#endregion
#region 生命周期
    private void Awake()
    {
        exitAnimDuration = 1.2f;
    }
#endregion

#region 过场动画
    void EnterBackground()
    {
        FadeIn(background, 0.8f);
    }
    void EnterBackgroundRing()
    {
        if (bgRing == null) return;

        FadeIn(bgRing, 0.8f);
        RotateToZero(bgRing.rectTransform, 5f, 1f, IdleBackgroundRing);
    }
    void EnterMidRing()
    {
        if (midRing == null) return;

        ScaleIn(midRing, 1f);
        RotateToZero(midRing, 5f, 1f, IdleMidRing);
    }
    void EnterMidRedBox()
    {
        FadeInRotateIn(imagesMidRedBox, 0.6f, 10f);
    }
    void EnterForeYellowBox()
    {
        //  TODO:这两个动画不应该同时
        FadeInRotateIn(imagesForeYellowBox, 0.8f, 10f);
        ResetAndFillFadeIn(halosForeYellowBox, 0.2f);
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
    void ExitBackground()
    {
        FadeOut(background, 0.8f);
    }

    void ExitBackgroundRing()
    {
        if (bgRing == null) return;

        FadeOut(bgRing, 0.8f);
        RotateFromZero(bgRing.rectTransform, 5f, 1f);
    }

    void ExitMidRing()
    {
        if (midRing == null) return;
        ScaleOut(midRing, 1f);
        RotateFromZero(midRing, 5f, 1f);
    }

    void ExitMidRedBox()
    {
        FadeOutRotateOut(imagesMidRedBox, 0.6f, 10f);
    }

    void ExitForeYellowBox()
    {
        FadeOutRotateOut(imagesForeYellowBox, 0.8f, 10f);
        FadeOutFillOut(halosForeYellowBox, 0.2f);
    }

    void ExitHaloMask()
    {
        FadeOut(haloMask, 0.8f);
    }
    void ExitBlackMask()
    {
        FadeOut(blackMask, 0.4f);
    }
#endregion

#region 通用复用方法（只写一次）
    void FadeIn(Image img, float t) 
    { 
        if (img) 
            img.DOFade(1, t)
               .From(0)
               .SetEase(Ease.OutQuad); 
    }
    void FadeOut(Image img, float t) 
    { 
        if (img) 
            img.DOFade(0, t)
               .SetEase(Ease.OutQuad); 
    }

    void FadeIn(Image[] imgs, float t) 
    { 
        foreach (var i in imgs) 
            FadeIn(i, t); 
    }
    void FadeOut(Image[] imgs, float t) 
    { 
        foreach (var i in imgs) 
            FadeOut(i, t); 
    }

    void RotateToZero(Transform t, float from, float dur, TweenCallback onComplete = null)
    {
        t.DORotate(Vector3.zero, dur)
         .From(new Vector3(0, 0, from))
         .SetEase(Ease.OutQuad)
         .SetUpdate(true)
         .onComplete += onComplete;
    }

    void RotateFromZero(Transform t, float to, float dur)
    {
        t.DORotate(new Vector3(0, 0, to), dur)
        .SetEase(Ease.OutQuad)
        .SetUpdate(true);
    }

    void ScaleIn(Transform t, float dur) => t.DOScale(1, dur).From(0).SetEase(Ease.OutQuad);
    void ScaleOut(Transform t, float dur) => t.DOScale(0, dur).SetEase(Ease.OutQuad);

    void FadeInRotateIn(Image[] imgs, float fadeT, float rot) { foreach (var i in imgs) { FadeIn(i, fadeT); RotateToZero(i.rectTransform, rot, 0.8f); } }
    void FadeOutRotateOut(Image[] imgs, float fadeT, float rot) { foreach (var i in imgs) { FadeOut(i, fadeT); RotateFromZero(i.rectTransform, rot, 0.8f); } }

    void ResetAndFillFadeIn(Image[] imgs, float t)
    {
        foreach (var i in imgs) { 
            if (i) { 
                i.fillAmount = 0; 
                i.DOFillAmount(1, t).From(0); 
                FadeIn(i, t); 
            } 
        }
    }

    void FadeOutFillOut(Image[] imgs, float t)
    {
        foreach (var i in imgs) { 
            if (i) { 
                i.DOFillAmount(0, t); 
                FadeOut(i, t); 
            } 
        }
    }

    void KillAllLoopingAnimations()
    {
        if (bgRing) bgRing.rectTransform.DOKill();
        if (midRing) midRing.DOKill();
        if (haloMask) { haloMask.rectTransform.DOKill(); haloMask.DOKill(); }
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