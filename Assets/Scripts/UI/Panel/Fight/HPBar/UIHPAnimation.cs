using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//  动画
public class UIHPAnimation : MonoBehaviour
{
    [Header("动画设置")]
    [SerializeField] private float smoothTime = 0.2f;

    private UIHPBinder _binder;
    private Image _foreground;

    private float _targetHP = 1f;
    private float _currentDisplayHP;
    private float _velocity;

    public void Init(UIHPBinder binder)
    {
        _binder = binder;
        _foreground = _binder.GetForegroundImage();
        _currentDisplayHP = 1f;
    }

    public void SetHPPercent(float percent)
    {
        _targetHP = Mathf.Clamp01(percent);
        StopAllCoroutines();
        StartCoroutine(HPAnimCoroutine());
    }

    IEnumerator HPAnimCoroutine()
    {
        while (Mathf.Abs(_targetHP - _currentDisplayHP) > 1e-6)
        {
            _currentDisplayHP = Mathf.SmoothDamp(
                _currentDisplayHP,
                _targetHP,
                ref _velocity,
                smoothTime
            );
            
            if (_foreground != null)
                _foreground.fillAmount = _currentDisplayHP;
            
            yield return null;
        }

        _currentDisplayHP = _targetHP;
        if (_foreground != null)
            _foreground.fillAmount = _currentDisplayHP;
    }
}