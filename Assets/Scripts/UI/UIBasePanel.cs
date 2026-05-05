using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(RectTransform), typeof(UISoundPlayer))]
public class UIBasePanel : MonoBehaviour
{
    [Header("脚本音效")]
    [SerializeField] protected UISoundPlayer soundPlayer;
    [Header("动画参数")]
    private Coroutine _enterCoroutine;
    protected Sequence _seq;
    [Tooltip("进场开始时间")]
    [SerializeField] protected float enterTime = 0f;

    [Tooltip("退场开始时间（进场结束时间）")]
    [SerializeField] protected float exitTime = 1f;

    [Tooltip("退场动画时长")]
    [SerializeField] protected float exitAnimDuration = 1f;
#region 状态标记
    // 动画排队标记
    protected bool _isPlayingAnimation = false;
    private bool _shouldBeVisible = true;
#endregion
#region 生命周期
    protected virtual void Awake()
    {
        if (soundPlayer == null) 
            soundPlayer = GetComponent<UISoundPlayer>();
    }
#endregion
#region 开关回调
    protected Action OnOpenComplete;
    protected Action OnCloseComplete;
    public void RegisterOnOpenComplete(Action callback)
    {
        OnOpenComplete += callback;
    }
    public void UnregisterOnOpenComplete(Action callback)
    {
        OnOpenComplete -= callback;
    }
    protected void TriggerOnOpenComplete()
    {
        OnOpenComplete?.Invoke();
        OnOpenComplete = null;
    }
    public void RegisterOnCloseComplete(Action callback)
    {
        OnCloseComplete += callback;
    }

    public void UnregisterOnCloseComplete(Action callback)
    {
        OnCloseComplete -= callback;
    }
    protected void TriggerOnCloseComplete()
    {
        OnCloseComplete?.Invoke();
        OnCloseComplete = null;
    }
#endregion
#region 面板操作
    public virtual void OpenPanel(string name, bool visible)
    {
        _shouldBeVisible = visible;
        gameObject.SetActive(visible);
        if (!visible) return;
        if (soundPlayer)
            soundPlayer.PlayOpenSoundManually();
        PlayEnterAnimation();
    }
    public virtual void OpenPanel(string name)
    {
        OpenPanel(name, true);
    }

    public virtual void ClosePanel()
    {
        if (_isPlayingAnimation)
        {
            return;
        }

        if (_enterCoroutine != null)
        {
            StopCoroutine(_enterCoroutine);
            _enterCoroutine = null;
        }
        if (soundPlayer)
            soundPlayer.PlayCloseSoundManually();
        PlayExitAnimation(true); // 关闭=销毁
    }
    public virtual void HidePanel()
    {
        if (_isPlayingAnimation)
        {
            return;
        }
        _shouldBeVisible = false;
        if (soundPlayer)
            soundPlayer.PlayCloseSoundManually();
        PlayExitAnimation(false); // 隐藏=不销毁
    }

    public virtual void ShowPanel()
    {
        if (_shouldBeVisible) return;
        _shouldBeVisible = true;
        gameObject.SetActive(true);
        if (soundPlayer)
            soundPlayer.PlayOpenSoundManually();
        PlayEnterAnimation();
    }
#endregion
#region 过场动画相关
    protected virtual void PlayEnterAnimation()
    {
        if (_seq != null)
        {
            _seq.Kill();
            _seq = null;
        }
        _seq = DOTween.Sequence();

        _isPlayingAnimation = true;
        //  TODO: Add Tweens here.

        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;

            TriggerOnOpenComplete();
        });

        _seq.SetTarget(gameObject);
    }
    protected virtual void KillAllLoopingAnimations()
    {
        if (_seq == null) return;

        // _seq.Kill();
        _seq.Complete();

    }
    protected virtual void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;
        KillAllLoopingAnimations();

        _seq = DOTween.Sequence();
        _seq.OnStart(() =>
        {
        });
        //  TODO: Add Tweens here.
        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            TriggerOnCloseComplete();
            if (destroyAfter)
                Destroy(gameObject);
            else HideImmediately();
        });

        _seq.SetTarget(gameObject);
    }

    protected void DestroyImmediate()
    {
        _isPlayingAnimation = false;

        gameObject.SetActive(false);
        Destroy(gameObject);

        TriggerOnCloseComplete();
    }
    protected void HideImmediately()
    {
        _isPlayingAnimation = false;
        _shouldBeVisible = false;

        gameObject.SetActive(false);
    }
#endregion
#region 生命周期
#endregion
#region 通用复用方法

    protected Tween MoveIn(Transform t, Vector3 from, float moveT)
    {
        return t.DOLocalMove(t.localPosition, moveT, true).From(from + t.localPosition).SetEase(Ease.InQuad);
    }
    protected Tween MoveIn(Transform[] transforms, Vector3 from, float moveT)
    {
        Sequence seq = DOTween.Sequence();
        foreach(var t in transforms)
            seq.Join(t.DOLocalMove(t.localPosition, moveT, true).From(from + t.localPosition).SetEase(Ease.InQuad));
        return seq;
    }

    protected Tween MoveOut(Transform t, Vector3 to, float moveT)
    {
        return t.DOLocalMove(to + t.localPosition, moveT, true).SetEase(Ease.OutQuad);
    }

    protected Tween FadeIn(Text text, float t)
    {
        if (text == null) return null;
        return text.DOFade(1, t).From(0).SetEase(Ease.OutQuad);
    }

    protected Tween FadeOut(Text text, float t)
    {
        if (text == null) return null;
        return text.DOFade(0, t).SetEase(Ease.OutQuad);
    }

    protected Tween FadeIn(Image img, float t)
    {
        if (img == null) return null;
        return img.DOFade(1, t).From(0).SetEase(Ease.OutQuad);
    }

    protected Tween FadeOut(Image img, float t)
    {
        if (img == null) return null;
        return img.DOFade(0, t).SetEase(Ease.OutQuad);
    }

    protected Tween FadeIn(Image[] imgs, float t)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs) if (i != null) seq.Join(FadeIn(i, t));
        return seq;
    }

    protected Tween FadeOut(Image[] imgs, float t)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs) if (i != null) seq.Join(FadeOut(i, t));
        return seq;
    }

    protected Tween RotateToZero(Transform t, float from, float dur)
    {
        if (t == null) return null;
        return t.DORotate(Vector3.zero, dur)
                .From(new Vector3(0, 0, from))
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
    }

    protected Tween RotateFromZero(Transform t, float to, float dur)
    {
        if (t == null) return null;
        return t.DORotate(new Vector3(0, 0, to), dur)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
    }

    protected Tween ScaleIn(Transform t, float dur)
    {
        if (t == null) return null;
        return t.DOScale(1, dur).From(0).SetEase(Ease.OutQuad);
    }

    protected Tween ScaleOut(Transform t, float dur)
    {
        if (t == null) return null;
        return t.DOScale(0, dur).SetEase(Ease.OutQuad);
    }

    protected Tween FadeInRotateIn(Image[] imgs, float fadeT, float rot, float rotateT)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs)
        {
            if (i == null) continue;
            seq.Join(FadeIn(i, fadeT));
            seq.Join(RotateToZero(i.rectTransform, rot, rotateT));
        }
        return seq;
    }

    protected Tween FadeInRotateIn(Image img, float fadeT, float rot, float rotateT)
    {
        Sequence seq = DOTween.Sequence();
            if (img == null) return null;
            seq.Join(FadeIn(img, fadeT));
            seq.Join(RotateToZero(img.rectTransform, rot, rotateT));
        return seq;
    }

    protected Tween FadeOutRotateOut(Image[] imgs, float fadeT, float rot, float rotateT)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs)
        {
            if (i == null) continue;
            seq.Join(FadeOut(i, fadeT));
            seq.Join(RotateFromZero(i.rectTransform, rot, rotateT));
        }
        return seq;
    }

    protected Tween FadeOutRotateOut(Image img, float fadeT, float rot, float rotateT)
    {
        Sequence seq = DOTween.Sequence();
            if (img == null) return null;
            seq.Join(FadeOut(img, fadeT));
            seq.Join(RotateFromZero(img.rectTransform, rot, rotateT));
        return seq;
    }
    protected Tween FillIn(Image image, float t)
    {
        if (image == null) return null;
        image.fillAmount = 0;
        return image.DOFillAmount(1, t).From(0);
    }
    protected Tween FillIn(Image[] imgs, float t)
    {
        Sequence seq = DOTween.Sequence();
        foreach(var i in imgs)
        {
            if (i == null) continue;
            seq.Join(FillIn(i, t));
        }
        return seq;
    }
    protected Tween ResetAndFillFadeIn(Image[] imgs, float t)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs)
        {
            if (i == null) continue;
            seq.Join(FillIn(i, t));
            seq.Join(FadeIn(i, t));
        }
        return seq;
    }

    protected Tween ResetAndFillFadeIn(Image img, float t)
    {
        Sequence seq = DOTween.Sequence();

            if (img == null) return null;
            img.fillAmount = 0;
            seq.Join(img.DOFillAmount(1, t).From(0));
            seq.Join(FadeIn(img, t));
        return seq;
    }

    protected Tween FadeOutFillOut(Image[] imgs, float t)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs)
        {
            if (i == null) continue;
            seq.Join(i.DOFillAmount(0, t));
            seq.Join(FadeOut(i, t));
        }
        return seq;
    }
    protected Tween FadeOutFillOut(Image img, float t)
    {
        Sequence seq = DOTween.Sequence();
            if (img == null) return null;
            seq.Join(img.DOFillAmount(0, t));
            seq.Join(FadeOut(img, t));
        return seq;
    }
    
    /* Inspictor窗体绑定 */
    public void OnButtonSelected(Button button)
    {
        if (button == null) return;
        if (_isPlayingAnimation) return;
        button.transform.DOKill();
        button.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutSine);
    }
    public void OnButtonDeselected(Button button)
    {
        if (button == null) return;
        if (_isPlayingAnimation) return;
        button.transform.DOKill();
        button.transform.DOScale(1f, 0.1f).SetEase(Ease.InSine);
    }
#endregion
}