using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class UIPanelEcho : UIBasePanel
{
    [Header("对话数据")]
    [Header("对话UI脚本")]
    public UICommunication uiCommunication;
    [Header("归属的NPC")]
    public IDialogueTrigger dialogueTrigger;
    //  面板动画
    [Header("动画组件")]
    [Header("动画参数")]
    // 动画配置（可复用，改数字就行）
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float rotateDuration = 0.5f;
    [SerializeField] private float scaleDuration = 0.5f;
    private Sequence _seq;
    [Header("红色文本框")]
    // [SerializeField] private Transform transMidRedBox;
    [SerializeField] private Image[] imagesMidRedBox;
    [Header("黄色文本框")]
    // [SerializeField] private Transform transForeYellowBox;
    [SerializeField] private Image[] imagesForeYellowBox;
    [SerializeField] private Image[] halosForeYellowBox;
    [Header("装饰层")]
    [SerializeField] private Image blackMask;
    [SerializeField] private Text[] texts;
    private Action _onPanelReady;
#region 覆写动画
    protected override void PlayEnterAnimation()
    {
        _isPlayingAnimation = true;

        _seq.Join(EnterMidRedBox());
        _seq.Join(EnterForeYellowBox());
        _seq.Append(EnterTexts());

        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            _onPanelReady?.Invoke();
            _onPanelReady = null;
        });

        _seq.SetTarget(gameObject);
    }

    void KillAllLoopingAnimations()
    {
        if(_seq == null) return;
        _seq.Kill();
    }
    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;
        KillAllLoopingAnimations();

        _seq = DOTween.Sequence();
        _seq.Append(ExitTexts());
        _seq.Join(ExitBlackMask());
        _seq.Join(ExitForeYellowBox());
        _seq.Join(ExitMidRedBox());

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
        exitAnimDuration = 1.2f;
        _seq =DOTween.Sequence();
        // EventBus.Instance.Subscribe<DialogueStartEvent>(OnDialogue);
    }
#endregion

#region 过场动画

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
    
    Tween EnterTexts()
    {
        Sequence seq = DOTween.Sequence();
        foreach(var t in texts)
        {
            seq.Join(FadeIn(t, (float)0.2 * fadeDuration));
        }
        return seq;
    }
#endregion

#region 退场动画
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
    Tween ExitBlackMask()
    {
        return FadeOut(blackMask, (float)0.5 * fadeDuration);
    }

    Tween ExitTexts()
    {
        Sequence seq = DOTween.Sequence();
        foreach(var t in texts)
        {
            seq.Join(FadeOut(t, (float)0.2 * fadeDuration));
        }
        return seq;
    }
#endregion

#region 业务
    /// 绑定发起对话的来源
    private void BindDialogueSource(IDialogueTrigger trigger)
    {
        dialogueTrigger = trigger;
    }
    /// 显示对话UI
    private void ShowDialogue(List<KeyValuePair<int, string>> lines)
    {
        gameObject.SetActive(true);
        _onPanelReady += () => uiCommunication.StartDialogue(lines);
    }
    /// 绑定对话者
    private void BindSpeaker(string[] speakers)
    {
        uiCommunication.SetSpeakers(speakers);
    }

    // public void OnDialogue(DialogueStartEvent evt)
    public void OnDialogue(IDialogueTrigger trigger)
    {
        BindDialogueSource(trigger);
        ShowDialogue(trigger.GetDialogueLines());
        BindSpeaker(trigger.GetSpeaker());
    }
#endregion
}
