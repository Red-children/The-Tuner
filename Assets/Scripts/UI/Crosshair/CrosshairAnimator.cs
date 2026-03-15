using UnityEngine;
using UnityEngine.UI;

public class CrosshairAnimator : MonoBehaviour
{
    //小准星依旧用动画系统控制
    public Animator _animSmall;          // 小准星 Animator

    [Header("大准星动画控制")]
    public float maxScale = 1.2f;
    public float minScale = 0.8f;
    public AnimationCurve scaleCurve;    // 可选，自定义缩放曲线

    private Image _crosshairBig;          // 大准星 Image
    private bool _isCritical = false;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        CrosshairBigScaleAnimation();
    }

    public void SetCriticalState(bool isCritical)
    {
        _isCritical = isCritical;
    }

    #region 精准命中
    public void PlayHitAnimation(bool isCritical, double currentTime)
    {
        if (isCritical && _animSmall != null)
        {
            _animSmall.Play("PreciseHit", 0, 0f);
            Debug.Log($"精准命中动画，时间 {currentTime:F1}");
        }
    }
    #endregion

    #region 初始化
    public void Init()
    {
        // 获取大准星 Image
        Transform big = transform.Find("CrosshairBig");
        if (big != null) _crosshairBig = big.GetComponent<Image>();
        if (_crosshairBig == null) Debug.LogError("找不到大准星 Image");

        // 获取小准星 Animator（如果未手动赋值）
        if (_animSmall == null)
        {
            Transform small = transform.Find("CrosshairSmall");
            if (small != null) _animSmall = small.GetComponent<Animator>();
        }
    }
    #endregion

    #region 大准星缩放动画
    public void CrosshairBigScaleAnimation() 
    {
        // 每帧更新大准星缩放
        if (RhythmManager.Instance != null && _crosshairBig != null)
        {
            float progress = (float)RhythmManager.Instance.BeatProgress; // 0~1
            float t = scaleCurve != null ? scaleCurve.Evaluate(progress) : progress;
            float scale = Mathf.Lerp(maxScale, minScale, t);
            _crosshairBig.rectTransform.localScale = Vector3.one * scale;
        }
    }

    #endregion

}