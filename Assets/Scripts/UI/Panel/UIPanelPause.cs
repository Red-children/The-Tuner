using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
public class UIPanelPause : UIBasePanel
{
    [Header("动画组件")]
    [Header("背景底色")]
    [SerializeField] private Image background;
    [Header("中层圆")]
    [SerializeField] private Image midCircleInnerMain;
    [SerializeField] private Image midCircleInnerDot;
    [SerializeField] private Image midCirlcleOuter;
    [SerializeField] private Image midCircleOuterHalo;
    [Header("顶部菱形")]
    [SerializeField] private Image midTopDiamond;
    [Header("底部菱形")]
    [SerializeField] private Image midBottomDiamond;
    [Header("小装饰")]
    [SerializeField] private Image docStar;
    [SerializeField] private Image[] docCubes;
    [SerializeField] private Image[] docNotes;
    [Header("按钮")]
    [SerializeField] private Button buttonCountinue;
    [SerializeField] private Button buttonReturn;
    [SerializeField] private Button buttonReset;

    #region 覆写动画
    protected override void PlayEnterAnimation()
    {
        _isPlayingAnimation = true;

        Sequence seq = DOTween.Sequence();
        seq.Append(EnterBackgound());
        seq.AppendCallback(IdleDocStar);
        seq.AppendCallback(IdleMidCircle);
        seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
        });

        seq.SetTarget(gameObject);
    }
    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;
        // KillAllLoopingAnimations();

        Sequence seq = DOTween.Sequence();

        seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            OnCloseComplete?.Invoke();

            Destroy(gameObject);
        });

        seq.SetTarget(gameObject);
    }
#endregion

#region 过场动画
    Tween EnterBackgound()
    {
        if (background == null) return null;
        return FadeIn(background, 0.5f);
    }
#endregion

#region 驻场动画
    void IdleDocStar()
    {
        if(docStar == null) return;
        docStar.DOFade(0.5f,1.5f)
               .SetLoops(-1,LoopType.Yoyo)
               .SetEase(Ease.Linear)
               .SetUpdate(true);
    }
    void IdleMidCircle()
    {
        if (midCircleInnerMain == null) return;
        if (midCircleInnerDot == null) return;
        if (midCirlcleOuter == null) return;
        if (midCircleOuterHalo == null) return;
        midCircleInnerMain.rectTransform
            .DORotate(new Vector3(0, 0, 720), 5f, RotateMode.FastBeyond360)
            .SetLoops(-1,LoopType.Yoyo)
            .SetEase(Ease.InOutElastic)
            .SetUpdate(true);
        midCircleInnerDot.rectTransform
            .DORotate(new Vector3(0, 0, -720), 5f,RotateMode.FastBeyond360)
            .SetLoops(-1,LoopType.Yoyo)
            .SetEase(Ease.InOutElastic)
            .SetUpdate(true);
    }
#endregion

#region 按钮回调
    /* Inspictor窗体绑定 */
    public void OnButtonSelected(Button button)
    {
        if (button == null) return;
        button.transform.DOKill();
        button.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutSine);
    }
    public void OnButtonDeselected(Button button)
    {
        if (button == null) return;
        button.transform.DOKill();
        button.transform.DOScale(1f, 0.1f).SetEase(Ease.InSine);
    }

    public void OnClickCountinue()
    {
        //  TODO:
    }
    public void OnClickReturn()
    {
        //  TODO:
    }
    public void OnClickReset()
    {
        //  TODO:
    }
    #endregion
}
