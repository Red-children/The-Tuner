using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIComboInfo : MonoBehaviour
{
    // 冷却条
    public UIComboInfoBar bar;
    // 文本
    public UIComboInfoText text;
    
    [Header("连击配置")]
    public float coolDownTime = 3f;     // 连击超时时间
    
    private void Init()
    {
        if (bar == null)
        {
            bar = GetComponentInChildren<UIComboInfoBar>();
        }
        if (text == null)
        {
            text = GetComponentInChildren<UIComboInfoText>();
        }

        if (bar == null || text == null)
        {
            Debug.LogError("UIComboInfo 组件缺失!!!!");
            return;
        }

        // 设置冷却条持续时间
        bar.duration = coolDownTime;
        
        // 注册连击事件
        EventBus.Instance.Subscribe<ComboChangedEvent>(OnComboChanged);
        EventBus.Instance.Subscribe<ComboBreakEvent>(OnComboBreak);
        
        // 初始状态
        UpdateComboDisplay(0);
        bar.StopCoolDown();
    }

    /// <summary>
    /// 连击数变化时的处理
    /// </summary>
    void OnComboChanged(ComboChangedEvent evt)
    {
        UpdateComboDisplay(evt.ComboData.CurrentCombo);
        
        // 如果有连击数，启动冷却条
        if (evt.ComboData.CurrentCombo > 0)
        {
            Debug.Log("UIComboInfo 启用冷却提示条");
            bar.StartOrResetCoolDown();
        }
        else
        {
            bar.StopCoolDown();
        }
        
        // 显示连击效果提示
        if (evt.ComboData.HasEffects)
        {
            ShowComboEffects(evt.ComboData.Effects);
        }
    }

    /// <summary>
    /// 连击中断时的处理
    /// </summary>
    void OnComboBreak(ComboBreakEvent evt)
    {
        Debug.Log($"连击中断，最终连击数: {evt.FinalCombo}");
        
        // 可以在这里添加连击中断的特效
        if (evt.FinalCombo >= 10)
        {
            // 高连击中断的特殊效果
            text.TextAnimation(RhythmRank.Perfect);
        }
    }

    /// <summary>
    /// 更新连击显示
    /// </summary>
    private void UpdateComboDisplay(int comboCount)
    {
        text.SetDisplayText(comboCount.ToString());
        
        // 根据连击数改变文本颜色（通过TextMeshProUGUI的color属性）
        if (text.text != null)
        {
            if (comboCount >= 15)
            {
                text.text.color = Color.red; // 高连击红色
            }
            else if (comboCount >= 10)
            {
                text.text.color = Color.yellow; // 中等连击黄色
            }
            else
            {
                text.text.color = Color.white; // 普通连击白色
            }
        }
    }

    /// <summary>
    /// 显示连击效果提示
    /// </summary>
    private void ShowComboEffects(ComboEffect[] effects)
    {
        foreach (var effect in effects)
        {
            switch (effect)
            {
                case ComboEffect.BulletPenetration:
                    Debug.Log("连击效果: 子弹穿透已激活");
                    // 可以在这里添加子弹穿透的UI提示
                    break;
                    
                case ComboEffect.Invincibility:
                    Debug.Log("连击效果: 无敌状态已激活");
                    // 可以在这里添加无敌状态的UI提示
                    break;
            }
        }
    }

    #region 生命周期
    void Start()
    {
        Init();
    }
    
    void OnDestroy()
    {
        // 取消事件注册
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<ComboChangedEvent>(OnComboChanged);
            EventBus.Instance.Unsubscribe<ComboBreakEvent>(OnComboBreak);
        }
    }
    #endregion
}