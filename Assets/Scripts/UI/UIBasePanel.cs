using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class UIBasePanel : MonoBehaviour
{
    private bool _isRemoved = false;
    private string _name;
    private Coroutine _enterCoroutine;

    [Header("Timeline 动画")]
    [SerializeField] protected PlayableDirector playableDirector;

    [Header("进场开始时间")]
    [SerializeField] protected float enterTime = 0f;

    [Header("退场开始时间（进场结束时间）")]
    [SerializeField] protected float exitTime = 1f;

    [Header("退场动画时长")]
    [SerializeField] protected float exitAnimDuration = 1f;
#region 状态标记
    // 动画排队标记
    protected bool _isPlayingAnimation = false;
    private bool _pendingClose = false;
    private bool _shouldBeVisible = true;
    private bool _pendingHide = false;
    public Action OnCloseComplete;
#endregion
#region 面板操作
    public virtual void OpenPanel(string name)
    {
        _name = name;
        _shouldBeVisible = true;
        gameObject.SetActive(true);
        _isRemoved = false;
        _pendingClose = false;
        _pendingHide = false;

        // if (playableDirector == null || playableDirector.playableAsset == null)
        //     return;

        PlayEnterAnimation();
    }

    public virtual void ClosePanel()
    {
        if (_isPlayingAnimation)
        {
            _pendingClose = true;
            return;
        }

        _isRemoved = true;

        if (_enterCoroutine != null)
        {
            StopCoroutine(_enterCoroutine);
            _enterCoroutine = null;
        }

        PlayExitAnimation(true); // 关闭=销毁
    }
    public virtual void HidePanel()
    {
        if (_isPlayingAnimation)
        {
            _pendingHide = true;
            return;
        }

        PlayExitAnimation(false); // 隐藏=不销毁
    }

    public virtual void ShowPanel()
    {
        if (_shouldBeVisible) return;
        _shouldBeVisible = true;
        gameObject.SetActive(true);
        _pendingHide = false;

        if (playableDirector != null && playableDirector.playableAsset != null)
        {
            PlayEnterAnimation();
        }
    }
#endregion
#region 过场动画相关
    protected virtual void PlayEnterAnimation()
    {
        if (playableDirector == null) return;

        _isPlayingAnimation = true;
        playableDirector.time = enterTime;
        playableDirector.Play();
        _enterCoroutine = StartCoroutine(WaitAndPause(exitTime));
    }
    protected virtual void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;

        if (playableDirector == null)
        {
            if (destroyAfter)
                DestroyImmediate();
            else
                HideImmediately();
            return;
        }

        playableDirector.time = exitTime;
        playableDirector.Play();

        if (destroyAfter)
            Invoke(nameof(DestroyImmediate), exitAnimDuration);
        else
            Invoke(nameof(HideImmediately), exitAnimDuration);
    }

    private IEnumerator WaitAndPause(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (playableDirector != null)
        {
            playableDirector.Pause();
        }

        _isPlayingAnimation = false;
        _enterCoroutine = null;

        // 优先关闭 > 隐藏
        if (_pendingClose)
        {
            _pendingClose = false;
            ClosePanel();
        }
        else if (_pendingHide)
        {
            _pendingHide = false;
            HidePanel();
        }
    }

    protected void DestroyImmediate()
    {
        _isPlayingAnimation = false;
        _pendingClose = false;
        _pendingHide = false;

        gameObject.SetActive(false);
        Destroy(gameObject);

        OnCloseComplete?.Invoke();
    }
    protected void HideImmediately()
    {
        _isPlayingAnimation = false;
        _pendingClose = false;
        _pendingHide = false;
        _shouldBeVisible = false;

        gameObject.SetActive(false);
    }
#endregion
#region 生命周期
#endregion
#region 通用复用方法
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

    protected Tween FadeInRotateIn(Image[] imgs, float fadeT, float rot)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs)
        {
            if (i == null) continue;
            seq.Join(FadeIn(i, fadeT));
            seq.Join(RotateToZero(i.rectTransform, rot, 0.8f));
        }
        return seq;
    }

    protected Tween FadeOutRotateOut(Image[] imgs, float fadeT, float rot)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs)
        {
            if (i == null) continue;
            seq.Join(FadeOut(i, fadeT));
            seq.Join(RotateFromZero(i.rectTransform, rot, 0.8f));
        }
        return seq;
    }

    protected Tween ResetAndFillFadeIn(Image[] imgs, float t)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var i in imgs)
        {
            if (i == null) continue;
            i.fillAmount = 0;
            seq.Join(i.DOFillAmount(1, t).From(0));
            seq.Join(FadeIn(i, t));
        }
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
#endregion
}