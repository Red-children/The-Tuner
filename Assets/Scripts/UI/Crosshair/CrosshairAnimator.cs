using UnityEngine;
using UnityEngine.UI;

// 准星动画控制脚本（挂载在UICrosshair对象上）
public class CrosshairAnimator : MonoBehaviour
{
    
    public Animator _animSmall; // 小圆准星动画
    public Animator _animBig;   // 大圆环动画
    private CrosshairSpriteLoader _spriteLoader; // 依赖图片加载脚本

    // BGM相关数据（由主控脚本设置）
    private double _dspStartTime;
    private double _bgmProgress;
    private bool _isCritical = false;

    #region 生命周期
    private void Awake()
    {
        // 自动获取图片加载脚本
        _spriteLoader = gameObject.GetComponent<CrosshairSpriteLoader>();
        if (_spriteLoader == null)
        {
            Debug.LogError("CrosshairAnimator: 未找到CrosshairSpriteLoader脚本！");
            return;
        }

        // 初始化动画组件
        InitAnimators();
    }
    #endregion

    #region 对外接口（由主控脚本调用）
    // 设置BGM开始时间
    public void SetDspStartTime(double time)
    {
        _dspStartTime = time;
        Debug.Log("CrosshairAnimator Received Dsptime");
    }

    // 设置暴击状态
    public void SetCriticalState(bool isCritical)
    {
        _isCritical = isCritical;
    }

    public void PlayHitAnimation(bool isCritical, double currentTime)
    {
        if (isCritical && _animSmall != null)
        {
            _animSmall.Play("PreciseHit", 0, 0f);
            Debug.Log($"精准命中动画，时间 {currentTime:F1}");
        }
    }

    // 更新BGM进度
    public void UpdateBgmProgress(double progress)
    {
        _bgmProgress = progress;
    }

    // 播放命中动画


    // 播放缩放动画（指示器激活时）
    public void PlayScaleAnimation(BeatPreviewEvent evt)
    {
        if (_animBig == null) return;

        Debug.Log($"[CrosshairAnimator] 播放缩放动画，Time.time = {Time.time:F3}");
        _animBig.Play("Indicator", 0, 0f);
    }

    // 更新闲置状态动画
    public void UpdateIdleState()
    {
    }
    #endregion

    #region 内部逻辑
    // 初始化动画组件
    private void InitAnimators()
    {
        _animBig = transform.Find("CrosshairBig").GetComponent<Animator>();
        _animSmall = transform.Find("CrosshairSmall").GetComponent<Animator>();
    }

    // 可扩展：普通命中动画
    private void PlayNormalHitAnimation()
    {
        if (_animSmall != null)
        {
            _animSmall.Play("NormalHit");
        }
    }
    #endregion

    public void OnAnimationComplete()
    {
        Debug.Log("[CrosshairAnimator] 动画完整播放结束");
    }
}