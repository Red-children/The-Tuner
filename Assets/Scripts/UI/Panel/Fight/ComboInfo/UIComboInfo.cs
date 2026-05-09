using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIComboInfo : MonoBehaviour
{
    [System.Serializable]
    struct ComboInfo
    {
        public Image background;
        public TextMeshProUGUI text;
        public Image decoration;
    }
    
    [SerializeField] private ComboInfo comboInfo;
    [SerializeField] private UIComboInfoText text;  // 保留文本动画组件
    
    [Header("间歇时间")]
    public float coolDownTime = 1f;
    
    private Tween _coolDownTween;
    private int _currentCombo = 0;
    
    void Init()
    {
        if (comboInfo.background == null)
        {
            Debug.LogError("UIComboInfo: background组件缺失!!!!");
            return;
        }
        
        if (comboInfo.text == null)
        {
            Debug.LogError("UIComboInfo: text组件缺失!!!!");
            return;
        }
        
        if (text == null)
        {
            text = GetComponentInChildren<UIComboInfoText>();
        }
        
        // 初始状态：背景条隐藏
        comboInfo.background.fillAmount = 0f;
        comboInfo.background.gameObject.SetActive(false);
        
        // 初始状态：文本隐藏（不显示"0"）
        comboInfo.text.gameObject.SetActive(false);
        // 同时确保 UIComboInfoText 组件中的文本也隐藏（如果有独立的 TextMeshProUGUI 引用）
        if (text != null)
        {
            text.SetDisplayText("0");
            text.gameObject.SetActive(false);
        }

        EventBus.Instance.Subscribe<ComboChangedEvent>(OnComboChanged);
        EventBus.Instance.Subscribe<ComboBreakEvent>(OnComboBreak);
    }
    
    // 重置计数器
    void ResetCounter()
    {
        text.SetDisplayText("0");
        StopCoolDownAnimation();
        comboInfo.background.gameObject.SetActive(false);
        // 重置时隐藏文本
        comboInfo.text.gameObject.SetActive(false);
        if (text != null)
        {
            text.gameObject.SetActive(false);
        }
        _currentCombo = 0;
    }
    
    // 设置计数器显示
    void SetCounter(int num)
    {
        _currentCombo = num;
        text.SetDisplayText(num.ToString());
        
        // 当连击数 > 0 时显示文本，否则隐藏
        if (num > 0)
        {
            comboInfo.text.gameObject.SetActive(true);
            if (text != null)
            {
                text.gameObject.SetActive(true);
            }
        }
        else
        {
            comboInfo.text.gameObject.SetActive(false);
            if (text != null)
            {
                text.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 开始冷却填充动画（从0填充到1，表示冷却进行中）
    /// </summary>
    void StartCoolDownAnimation()
    {
        // 停止之前的动画
        _coolDownTween?.Kill();
        
        // 显示背景条
        comboInfo.background.gameObject.SetActive(true);
        
        // 重置初始值
        comboInfo.background.fillAmount = 0f;
        
        // DOTween动画：从0填充到1，duration = coolDownTime
        _coolDownTween = comboInfo.background
            .DOFillAmount(1f, coolDownTime)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 动画完成后的回调（可选，比如隐藏背景）
                comboInfo.background.gameObject.SetActive(false);
            });
    }
    
    /// <summary>
    /// 停止冷却动画并重置
    /// </summary>
    void StopCoolDownAnimation()
    {
        _coolDownTween?.Kill();
        comboInfo.background.fillAmount = 0f;
        comboInfo.background.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 重置并重新开始冷却动画（用于连续命中时重置计时器）
    /// </summary>
    void ResetAndRestartCoolDown()
    {
        // 重置动画并重新开始
        _coolDownTween?.Kill();
        comboInfo.background.fillAmount = 0f;
        
        _coolDownTween = comboInfo.background
            .DOFillAmount(1f, coolDownTime)
            .SetEase(Ease.Linear);
    }

#region 回调函数
    void OnComboChanged(ComboChangedEvent evt)
    {
        coolDownTime = evt.comboTimeout;
        SetCounter(evt.newCombo);

        if (evt.newCombo > 0)
        {
            ResetAndRestartCoolDown();
            text.TextAnimation(evt.rank);
        }
        else
        {
            StopCoolDownAnimation();
        }

        CancelInvoke(nameof(ResetCounter));
        Invoke(nameof(ResetCounter), coolDownTime);
    }
    
    void OnComboBreak(ComboBreakEvent evt)
    {
        ResetCounter();
        text.TextAnimation(evt.rank);
    }
#endregion

#region 生命周期
    void Start()
    {
        Init();
    }
    
    void OnDestroy()
    {
        // 取消事件订阅，避免访问已销毁的对象
        EventBus.Instance.Unsubscribe<ComboChangedEvent>(OnComboChanged);
        EventBus.Instance.Unsubscribe<ComboBreakEvent>(OnComboBreak);
        
        // 清理动画，避免内存泄漏
        _coolDownTween?.Kill();
    }
#endregion
}