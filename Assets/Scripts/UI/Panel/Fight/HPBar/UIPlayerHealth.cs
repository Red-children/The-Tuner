using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPlayerHealth : MonoBehaviour
{
    [System.Serializable]
    struct PlayerHealth
    {
        public Image background;
        public Image bottom;
        public Image title;   // "LIFE" 艺术字图片
        public Image fill;    // 血条填充图片
        public Image side;
    }

    [SerializeField] private PlayerHealth playerHealth;
    
    [Header("动画设置")]
    [SerializeField] private float smoothDuration = 0.3f;
    [SerializeField] private float lowHpThreshold = 0.3f;
    
    private float _maxHP;
    private float _currentHP;
    private Tween _currentFillTween;
    
    #region 初始化
    private void Awake()
    {
        InitFillImage();
    }
    
    private void Start()
    {
        RefreshHP();
    }
    
    private void InitFillImage()
    {
        if (playerHealth.fill != null)
        {
            playerHealth.fill.fillAmount = 1f;
            playerHealth.fill.type = Image.Type.Filled;
            playerHealth.fill.fillMethod = Image.FillMethod.Horizontal;
        }
    }
    
    private void RefreshHP()
    {
        PlayerStats player = GameObject.FindWithTag("Player")?.GetComponent<PlayerStats>();
        if (player == null) return;
        
        _maxHP = player.MaxHealth;
        _currentHP = player.CurrentHealth;
        
        float percent = _currentHP / _maxHP;
        
        // 直接设置（无动画）
        SetFillAmount(percent);
    }
    #endregion
    
    #region 血量更新
    private void OnHPChanged(PlayerHealthChangedEventStruct evt)
    {
        _maxHP = evt.maxHealth;
        _currentHP = evt.currentHealth;
        
        // 播放填充动画
        AnimateFillAmount(evt.healthPercent);
        
        // 低血量警告效果
        if (evt.healthPercent <= lowHpThreshold)
        {
            PlayLowHpWarning();
        }
    }
    
    private void SetFillAmount(float percent)
    {
        if (playerHealth.fill != null)
        {
            playerHealth.fill.fillAmount = percent;
        }
    }
    
    private void AnimateFillAmount(float targetPercent)
    {
        // 停止当前动画
        _currentFillTween?.Kill();
        
        if (playerHealth.fill == null) return;
        
        // 使用 DOTween 平滑过渡
        _currentFillTween = playerHealth.fill
            .DOFillAmount(targetPercent, smoothDuration)
            .SetEase(Ease.OutCubic);
    }
    #endregion
    
    #region 警告效果
    private void PlayLowHpWarning()
    {
        if (playerHealth.fill == null) return;
        
        // 保存原始颜色
        Color originalColor = playerHealth.fill.color;
        
        // 红色闪烁警告
        playerHealth.fill.DOColor(Color.red, 0.15f)
            .SetLoops(4, LoopType.Yoyo)
            .OnComplete(() => playerHealth.fill.DOColor(originalColor, 0.15f));
        
        // side 图片闪烁
        if (playerHealth.side != null)
        {
            Color originalSideColor = playerHealth.side.color;
            playerHealth.side.DOColor(Color.red, 0.15f)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() => playerHealth.side.DOColor(originalSideColor, 0.15f));
        }
        
        // title 艺术字闪烁
        if (playerHealth.title != null)
        {
            Color originalTitleColor = playerHealth.title.color;
            playerHealth.title.DOColor(Color.red, 0.15f)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() => playerHealth.title.DOColor(originalTitleColor, 0.15f));
        }
    }
    
    /// <summary>
    /// 播放受伤闪白效果
    /// </summary>
    public void PlayHitFlash()
    {
        if (playerHealth.fill == null) return;
        
        Color originalColor = playerHealth.fill.color;
        playerHealth.fill.DOColor(Color.white, 0.05f)
            .OnComplete(() => playerHealth.fill.DOColor(originalColor, 0.1f));
    }
    
    /// <summary>
    /// 播放回血效果（绿色闪烁）
    /// </summary>
    public void PlayHealEffect()
    {
        if (playerHealth.fill == null) return;
        
        Color originalColor = playerHealth.fill.color;
        playerHealth.fill.DOColor(Color.green, 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() => playerHealth.fill.DOColor(originalColor, 0.1f));
    }
    
    /// <summary>
    /// 标题艺术字脉冲效果（可选的装饰动画）
    /// </summary>
    public void PlayTitlePulse()
    {
        if (playerHealth.title == null) return;
        
        playerHealth.title.transform
            .DOPunchScale(Vector3.one * 0.1f, 0.2f, vibrato: 1, elasticity: 1);
    }
    #endregion
    
    #region 事件订阅
    private void OnEnable()
    {
        EventBus.Instance.Subscribe<PlayerHealthChangedEventStruct>(OnHPChanged);
    }
    
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<PlayerHealthChangedEventStruct>(OnHPChanged);
    }
    
    private void OnDestroy()
    {
        _currentFillTween?.Kill();
    }
    #endregion
}