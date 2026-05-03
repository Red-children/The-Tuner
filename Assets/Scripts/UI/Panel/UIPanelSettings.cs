using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelSettings : UIBasePanel
{
#region 声明
    [Header("数据")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    // 缓存 Slider 与 SettingType 的对应关系
    private Dictionary<SettingType, Slider> _mapSlider;
    [Header("动画参数")]
    [SerializeField] private float rotateDuration = 0.2f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float scaleDuration = 0.3f;
    // private Sequence _seq;
    [Header("动画组件")]
    [Header("Background")]
    [SerializeField] private Image backgroundColor;
    [SerializeField] private Image backgroundMask;
    [SerializeField] private Image backgroundBlackLine;
    [Header("Mid")]
    [SerializeField] private Image midLineVerRed;
    [SerializeField] private Image midLineHorRed;
    [SerializeField] private Image midLineHorBlue;
    [SerializeField] private Image midLineVerBlue;
    [Header("Foreground")]
    [SerializeField] private Button buttonBack;
    [SerializeField] private Image settingsBackground;
    [SerializeField] private Image foregroundDecoration;
    [SerializeField] private RectTransform transSettingPanel;
    [SerializeField] private Button[] entries;
    [Header("ScreenMask")]
    [SerializeField] private Image screenMask;

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
        var layout = entries[0].GetComponentInParent<VerticalLayoutGroup>();
        layout.enabled = false;
        _seq.Append(EnterBackground());
        _seq.Join(EnterButtonBack());
        _seq.Join(EnterMidDecoration());
        _seq.Join(EnterSettingsPanel());
        _seq.Join(EnterEntries());
        _seq.OnComplete(() =>
        {
            layout.enabled = true; 
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
           screenMask.enabled = true; 
        });
        _seq.Join(ExitAnimation());
        _seq.OnComplete(() =>
        {
            screenMask.enabled = false;
            _isPlayingAnimation = false;
            TriggerOnCloseComplete();
            if (destroyAfter)
                Destroy(gameObject);
            else HideImmediately();
        });

        _seq.SetTarget(gameObject);
    }
#endregion
#region 过场动画
    Tween EnterBackground()
    {
        if(!backgroundColor || !backgroundMask || !backgroundBlackLine) return null;
        Sequence seq = DOTween.Sequence();
        seq.Join(FadeIn(backgroundColor, fadeDuration));
        seq.Join(FadeIn(backgroundMask, fadeDuration));
        seq.Join(FadeIn(backgroundBlackLine, fadeDuration));
        return seq;
    }
    Tween EnterMidDecoration()
    {
        if (!midLineHorBlue || !midLineHorRed || !midLineVerBlue || !midLineVerRed) return null;
        Sequence seq =DOTween.Sequence();
        seq.Join(FillIn(midLineHorBlue, fadeDuration));
        seq.Join(FillIn(midLineHorRed, fadeDuration));
        seq.Join(FillIn(midLineVerBlue, fadeDuration));
        seq.Join(FillIn(midLineVerRed, fadeDuration));
        return seq;
    }
    Tween EnterButtonBack()
    {
        if (buttonBack == null) return null;
        return ScaleIn(buttonBack.transform, scaleDuration);
    }
    Tween EnterSettingsPanel()
    {
        if (!settingsBackground || !foregroundDecoration || !transSettingPanel) return null;
        Sequence seq = DOTween.Sequence();
        seq.Join(FadeIn(settingsBackground, fadeDuration));
        seq.Join(FadeIn(foregroundDecoration, fadeDuration));
        seq.Join(RotateToZero(transSettingPanel, -6, rotateDuration));
        return seq;
    }
    Tween EnterEntries()
    {
        if (entries == null) return null;
        Sequence seq = DOTween.Sequence();
        foreach(var button in entries)
        {
            if (button == null) continue;
            seq.Join(MoveIn(button.image.rectTransform, new Vector3(0, -100, 0), fadeDuration));
        }
        return seq;
    }
#endregion
#region 退场动画
    Tween ExitAnimation()
    {
        if (screenMask == null) return null;
        return FadeIn(screenMask, fadeDuration);
    }
#endregion
#region 生命周期
    void Awake()
    {
        // _seq = DOTween.Sequence();
        exitAnimDuration = 1f;
    }
    void Start()
    {
        buttonBack.onClick.AddListener(OnButtonBackClick);

        BuildSliderMap();
        LoadAllSettings();
        RegisterCallbacks();
    }
    private void BuildSliderMap()
    {
        _mapSlider = new Dictionary<SettingType, Slider>
        {
            { SettingType.MasterVolume, masterVolumeSlider },
            { SettingType.SFXVolume, sfxVolumeSlider }
        };
    }
    
    private void LoadAllSettings()
    {
        var allSettings = SettingsManager.Instance.GetValues();
        
        foreach (var kvp in allSettings)
        {
            if (_mapSlider.TryGetValue(kvp.Key, out var slider))
            {
                slider.value = kvp.Value.amount;
            }
        }
    }
    
    private void RegisterCallbacks()
    {
        // 监听 Slider 变化
        masterVolumeSlider.onValueChanged.AddListener(val => 
            SettingsManager.Instance.SetValue(SettingType.MasterVolume, (int)val));
        
        sfxVolumeSlider.onValueChanged.AddListener(val => 
            SettingsManager.Instance.SetValue(SettingType.SFXVolume, (int)val));
        
        // 监听设置变化（其他系统修改时更新 UI）
        SettingsManager.Instance.RegisterCallback(SettingType.MasterVolume, () => 
            masterVolumeSlider.value = SettingsManager.Instance.GetValue(SettingType.MasterVolume));
        
        SettingsManager.Instance.RegisterCallback(SettingType.SFXVolume, () => 
            sfxVolumeSlider.value = SettingsManager.Instance.GetValue(SettingType.SFXVolume));
    }
#endregion
#region 按钮回调
    public void OnButtonBackClick()
    {
        UIManager.Instance.ClosePanel(this);
    }
#endregion
}
