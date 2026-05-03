using System.Collections;
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
    [SerializeField] private float rotateDuration = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float scaleDuration = 1f;
    private Sequence _seq;
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
#endregion

#region 覆写动画
    protected override void PlayEnterAnimation()
    {
        _isPlayingAnimation = true;

        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
        });

        _seq.SetTarget(gameObject);
    }
    private void KillAllLoopingAnimations()
    {
        if (_seq == null) return;

        _seq.Kill();

        // if (docStar) docStar.DOKill();
    }
    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;
        KillAllLoopingAnimations();

        _seq = DOTween.Sequence();

        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            // OnCloseComplete?.Invoke();
            TriggerOnCloseComplete();
            if (destroyAfter)
                Destroy(gameObject);
        });

        _seq.SetTarget(gameObject);
    }
#endregion

#region 生命周期
    void Awake()
    {
        _seq = DOTween.Sequence();
    }
    void Start()
    {
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
}
