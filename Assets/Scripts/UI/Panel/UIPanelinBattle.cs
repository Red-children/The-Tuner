using System.Security.Cryptography;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelinBattle : UIBasePanel
{
#region 声明
    [Header("动画参数")]
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float fadeDuration = 0.3f;
    [Header("动画组件")]
    [SerializeField] private RectTransform HPBar;
    [SerializeField] private RectTransform comboInfo;
    [SerializeField] private RectTransform weapenInfo;
    [SerializeField] private Image rankInfo;
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
        _seq.Join(EnterHPBar());
        _seq.Join(EnterComboInfo());
        _seq.Join(EnterRankInfo());
        _seq.Join(EnterWeaponInfo());
        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;

            TriggerOnOpenComplete();
        });

        _seq.SetTarget(gameObject);
    }
    protected override void KillAllLoopingAnimations()
    {
        if (_seq == null) return;

        _seq.Kill();

    }
    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;
        KillAllLoopingAnimations();

        _seq = DOTween.Sequence();
        _seq.OnStart(() =>
        {
        });
        _seq.Join(ExitHPBar());
        _seq.Join(ExitComboInfo());
        _seq.Join(ExitRankInfo());
        _seq.Join(ExitWeaponInfo());
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
#endregion
#region 进场动画
    Tween EnterHPBar()
    {
        if (HPBar == null) return null;
        return MoveIn(HPBar, new Vector3(-HPBar.rect.width, 0, 0), moveDuration);
    }
    Tween EnterWeaponInfo()
    {
        if (weapenInfo == null) return null;
        return MoveIn(weapenInfo, new Vector3(-weapenInfo.rect.width, 0, 0), moveDuration);
    }
    Tween EnterComboInfo()
    {
        if (comboInfo == null) return null;
        return MoveIn(comboInfo, new Vector3(-comboInfo.rect.width, 0, 0), moveDuration);
    }
    Tween EnterRankInfo()
    {
        if (rankInfo == null) return null;
        return FadeIn(rankInfo, fadeDuration);
    }
#endregion
#region 退场动画
    Tween ExitHPBar()
    {
        if (HPBar == null) return null;
        return MoveOut(HPBar, new Vector3(-HPBar.rect.width, 0, 0), moveDuration);
    }
    Tween ExitWeaponInfo()
    {
        if (weapenInfo == null) return null;
        return MoveOut(weapenInfo, new Vector3(-weapenInfo.rect.width, 0, 0), moveDuration);
    }
    Tween ExitComboInfo()
    {
        if (comboInfo == null) return null;
        return MoveOut(comboInfo, new Vector3(-comboInfo.rect.width, 0, 0), moveDuration);
    }
    Tween ExitRankInfo()
    {
        if (rankInfo == null) return null;
        return FadeOut(rankInfo, fadeDuration);
    }
#endregion
#region 生命周期
    void Awake()
    {

    }
#endregion
}
