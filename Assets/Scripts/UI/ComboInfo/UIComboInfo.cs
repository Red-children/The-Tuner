using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIComboInfo : MonoBehaviour
{
    //  冷却条
    public UIComboInfoBar bar;
    //  文本
    public UIComboInfoText text;
    [Header("间歇时间")]
    public float coolDownTime = 1f;     //  After coolDownTime seconds, _comboCount <= 0;
    // private int _comboCount = 0;        //  Count except Miss
    // private bool _isTriggered = false;  //  To known if Player Atk
    void Init()
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

        bar.duration = coolDownTime;

        EventBus.Instance.Subscribe<ComboChangedEvent>(OnComboChanged);
        EventBus.Instance.Subscribe<ComboBreakEvent>(OnComboBreak);
        
    }
    //  Miss(只给延迟重置用)
    void ResetCounter()
    {
        text.SetDisplayText("0");
        bar.StopCoolDown();
    }

    //  PreciseHit
    void SetCounter(int num)
    {
        text.SetDisplayText(num.ToString());
    }

#region 回调函数
    void OnComboChanged(ComboChangedEvent evt)
    {
        bar.duration = coolDownTime = evt.comboTimeout;

        SetCounter(evt.newCombo);

        if (evt.newCombo > 0)
        {
            bar.StartOrResetCoolDown();
            text.TextAnimation(evt.rank);
        }
        else
        {
            bar.StopCoolDown();
        }

        this.ResetTimer(nameof(ResetCounter), 1f);
        text.TextAnimation(evt.rank);
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
#endregion

}
