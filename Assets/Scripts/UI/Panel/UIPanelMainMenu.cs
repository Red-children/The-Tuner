using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public class UIPanelMainMenu : UIBasePanel
{
    [Header("动画组件")]
    [Header("动画参数")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float rotateDuration = 0.3f;
    [SerializeField] private float ScaleDuration = 0.3f;
    [SerializeField] private float MoveDuration = 0.1f;
    // private Sequence _seq;
    // Background
    [SerializeField] private Image backgroundColor;
    [SerializeField] private Image backgroundRedLine;
    [SerializeField] private Image backgroundBlueLine;
    [SerializeField] private Image disc;
    [SerializeField] private Image[] backgroundRings;
    // Mid
    [SerializeField] private Image arrow;
    [SerializeField] private Image[] midRings;
    [SerializeField] private Image title;
    [SerializeField] private Image[] cards;
    // Foreground
    //  主菜单按钮
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonSettings;
    // [SerializeField] private Button _btnExit;
    // Decoration
    [SerializeField] private Image score;
    [SerializeField] private Image gun;
    [SerializeField] private Image[] aroundButtonStart;
    [SerializeField] private Image target;
    // ScreenMask
    [SerializeField] private Image screenMask;
#region 流程控制标志
    private Action _onPanelReady;
#endregion
#region 初始化
    void Init()
    {
        // _seq = DOTween.Sequence();
        //  绑定按钮事件
        buttonStart.onClick.AddListener(OnStartClick);
        buttonSettings.onClick.AddListener(OnSettingsClick);
        // _btnExit.onClick.AddListener(OnExitClick);
    }
#endregion

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
        _seq.Append(EnterBackgroundColor());
        _seq.Join(EnterBackgroundRedLine());
        _seq.Join(EnterBackgroundBlueLine());
        _seq.Join(EnterDisc());
        _seq.Join(EnterBackgroundRings());
        _seq.Join(EnterArrow());
        _seq.Join(EnterMidRings());
        _seq.Join(EnterTitle());
        _seq.Join(EnterCards());
        _seq.Join(EnterButtonStart());
        _seq.Join(EnterButtonSettings());
        _seq.Join(EnterScore());
        _seq.Join(EnterGun());
        _seq.Join(EnterDecorations());
        _seq.Join(EnterTarget());
        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            // _onPanelReady?.Invoke();
            // _onPanelReady = null;
            TriggerOnOpenComplete();
        });
        _seq.SetTarget(gameObject);
    }
    protected override void KillAllLoopingAnimations()
    {
        if(_seq == null) return;
        _seq.Kill();

        if (target) target.rectTransform.DOKill();
        if (score) score.rectTransform.DOKill();
    }
    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;
        KillAllLoopingAnimations();

        _seq = DOTween.Sequence();
        _seq.OnStart(() =>
        {
           screenMask.enabled = true;
        });
        _seq.Append(ExitScreenMask());
        _seq.OnComplete(() =>
        {
            screenMask.enabled = false;
            _isPlayingAnimation = false;
            _seq.Kill();
            _seq = null;
            
            TriggerOnCloseComplete();
            if(destroyAfter)
                Destroy(gameObject);
            else HideImmediately();
        });
        _seq.SetTarget(gameObject);
    }

#endregion

#region 过场动画

    Tween EnterBackgroundColor()
    {
        if (backgroundColor == null) return null;
        return FadeIn(backgroundColor, fadeDuration);
    }
    Tween EnterBackgroundRedLine()
    {
        if (backgroundRedLine == null) return null;
        return ResetAndFillFadeIn(backgroundRedLine, fadeDuration);
    }
    Tween EnterBackgroundBlueLine()
    {
        if (backgroundBlueLine == null) return null;
        return ResetAndFillFadeIn(backgroundBlueLine, fadeDuration);
    }
    Tween EnterDisc()
    {
        if (disc == null) return null;
        return FadeIn(disc, fadeDuration);
    }
    Tween EnterBackgroundRings()
    {
        Sequence seq = DOTween.Sequence();
        foreach (var image in backgroundRings)
        {
            if (image == null) continue;
            seq.Append(RotateToZero(image.transform, 10, 0.25f * rotateDuration));
            seq.Join(FadeIn(image, 0.25f * rotateDuration));
        }
        return seq;
    }
    Tween EnterArrow()
    {
        if (arrow == null) return null;
        return MoveIn(arrow.transform, new Vector3(0, 0, 0), MoveDuration);
    }
    Tween EnterMidRings()
    {
        Sequence seq = DOTween.Sequence();
        foreach(var image in midRings)
        {
            if(image == null) continue;
            seq.Append(FadeInRotateIn(image, fadeDuration, -90f, rotateDuration));
        }
        return seq;
    }
    Tween EnterTitle()
    {
        if (title == null) return null;
        return ResetAndFillFadeIn(title, fadeDuration);
    }
    Tween EnterCards()
    {
        Sequence seq = DOTween.Sequence();
        foreach(var card in cards)
        {
            seq.Append(MoveIn(card.transform, new Vector3(0, 10, 0), MoveDuration));
        }
        return seq;
    }
    Tween EnterButtonStart()
    {
        if (buttonStart == null) return null;
        return MoveIn(buttonStart.transform, new Vector3(0, 10, 0), MoveDuration);
    }
    Tween EnterButtonSettings()
    {
        if (buttonSettings == null) return null;
        return MoveIn(buttonSettings.transform, new Vector3(0, 10, 0), MoveDuration);
    }
    Tween EnterScore()
    {
        if (score == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Append(ResetAndFillFadeIn(score, fadeDuration));
        seq.AppendCallback(IdleScore);
        return seq;
    }
    Tween EnterGun()
    {
        if (gun == null) return null;
        return MoveIn(gun.transform, new Vector3(10, 0, 0), MoveDuration);
    }
    Tween EnterDecorations()
    {
        Sequence seq = DOTween.Sequence();
        foreach(var img in aroundButtonStart)
        {
            if (img == null) continue;
            seq.Join(FadeIn(img, fadeDuration));
        }
        seq.AppendCallback(IdleTarget);
        return seq;
    }
    Tween EnterTarget()
    {
        if (target == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Append(FadeIn(target, fadeDuration));
        seq.OnComplete(IdleTarget);
        return seq;
    }
#endregion 

#region 空闲循环动画
    void IdleScore()
    {
        if (score == null) return;
        score.rectTransform.DOLocalMoveX(16f, 3f)
                .SetRelative(true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

        score.rectTransform.DOLocalMoveY(16f, 3.6f)
                .SetRelative(true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
    }
    void IdleTarget()
    {
        if (target == null) return;
        target.rectTransform.DOLocalMoveX(16f, 3f)
                .SetRelative(true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

        target.rectTransform.DOLocalMoveY(16f, 3.6f)
                .SetRelative(true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
    }
#endregion

#region 退场动画
    Tween ExitScreenMask()
    {
        if (screenMask == null) return null;
        return FadeIn(screenMask, fadeDuration);
    }
#endregion

#region 按钮回调函数
    private bool _buttonStart = true;
    private bool _buttonSettings = true;
    void OnStartClick()
    {
        if (_isPlayingAnimation) return;
        //TODO:
        // SceneManager.LoadScene("Test01");
        Debug.Log("Button Start Clicked");
    }
    void OnSettingsClick()
    {
        if (_isPlayingAnimation) return;
        if (!_buttonSettings) return;
        _buttonSettings = false;
        //TODO:
        Debug.Log("Button Settings Clicked");
        RegisterOnCloseComplete(() =>
        {
            var panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Settings);
            panel.RegisterOnCloseComplete(OnSettingsBack);
        });
        HidePanel();

    }
    void OnSettingsBack()
    {
        var panel = UIManager.Instance.GetPanel(UIManager.UIConst.Settings);
        panel.UnregisterOnCloseComplete(OnSettingsBack);
        ShowPanel();
        _buttonSettings = true;
    }
    // void OnExitClick()
    // {
    //     UIManager.Instance.ClosePanel(UIManager.UIConst.MainMenu);
    // }
#endregion

#region 生命周期
    void Awake()
    {
        Init();
        exitAnimDuration = 1f;
    }
#endregion
}
