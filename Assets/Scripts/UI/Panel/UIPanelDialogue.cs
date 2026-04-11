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
    [Header("底层图片")]
    [SerializeField] private Image[] background;
    [Header("底层环")]
    [SerializeField] private Image bgRing;
    [Header("红色文本框")]
    [SerializeField] private Transform transMidRedBox;
    [SerializeField] private Image[] imagesMidRedBox;
    [Header("中层环")]
    [SerializeField] private Transform midRing;
    [Header("黄色文本框")]
    [SerializeField] private Transform transForeYellowBox;
    [SerializeField] private Image[] imagesForeYellowBox;
    [SerializeField] private Image[] halosForeYellowBox;
    [Header("装饰层")]
    [SerializeField] private Image haloMask;

    protected override void PlayEnterAnimation()
    {
        _isPlayingAnimation = true;

        //  TODO:
        //  1.底部背景bg淡入
        EnterBackground();
        //  2.底部环bgRing顺时针旋转进场
        EnterBackgroundRing();
        //  3.中间层环midRing旋转放大进场
        EnterMidRing();
        //  4.中间层红色文本框midRedBox进场
        EnterMidBox();
    }
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

#region 过场动画
    void EnterBackground()
    {
        foreach(Image bg in background)
        {
            if(bg == null) return;

            bg.DOFade(1f, 0.8f).From(0f);
        }
    }
    void EnterBackgroundRing()
    {
        if (bgRing != null)
        {
            // 先把透明度归零
            Color originalColor = bgRing.color;
            originalColor.a = 0f;
            bgRing.color = originalColor;

            // 旋转（UI 绕 Z 轴转）
            bgRing.rectTransform
                .DORotate(new Vector3(0, 0, -360), 90f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
            // 同时淡入
            bgRing.DOFade(1f, 0.8f).SetEase(Ease.OutQuad);
        }
    }
    void EnterMidRing()
    {
        midRing.DOScale(1f, 0.8f).From(0f);
        midRing.DORotate(new Vector3(0, 0, -360), 45f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetUpdate(true);
    }
    void EnterMidBox()
    {
        foreach(Image image in imagesMidRedBox)
        {
            image.DOFade(1f, 0.8f).From(0f);
            image.rectTransform
                .DORotate(new Vector3(0f, 0f, 0f), 0.8f, RotateMode.FastBeyond360)
                .From(new Vector3(-40f, 0f, 0f))
                .SetEase(Ease.InQuad);
        }
    }
#endregion

#region 驻场动画

#endregion
}