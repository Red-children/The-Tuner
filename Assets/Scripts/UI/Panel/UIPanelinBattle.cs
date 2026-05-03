using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelinBattle : UIBasePanel
{
#region 声明
    // private Sequence _seq;
    [Header("动画组件")]
    [SerializeField] private Transform HPBar;
    [SerializeField] private Transform comboInfo;
    [SerializeField] private Transform weapenInfo;
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
        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
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
#region 生命周期
    void Awake()
    {

    }
#endregion
}
