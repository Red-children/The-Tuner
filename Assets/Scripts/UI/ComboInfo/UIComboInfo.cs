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
    private int _comboCount = 0;        //  Count except Miss
    private bool _isTriggered = false;  //  To known if Player Atk


    private RhythmRank _currentRank;    //  Rank of now
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

        _comboCount = 0;
        _isTriggered = false;
        bar.duration = coolDownTime;

        // bar.StopCoolDown();
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
        EventBus.Instance.Subscribe<PlayerAtkEvent>(OnPlayerAtk);
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
    }
    //  Miss(只给延迟重置用)
    void ResetCounter()
    {
        _comboCount = 0;
        text.SetDisplayText(_comboCount.ToString());
        bar.StopCoolDown();
    }

    //  PreciseHit
    void AddCounter(int num)
    {
        _comboCount += num;
        text.SetDisplayText(_comboCount.ToString());
    }

    void ResetTrigger()
    {
        _isTriggered = false;
    }
#region 回调函数
    void OnEnemyHit(EnemyHitEvent evt)
    {
        Debug.Log(_currentRank);
        Debug.Log($"UIComboInfo Received EnemyHitEvent\n_isTriggered = {_isTriggered}");
        if (_isTriggered)
        {   
            ResetTrigger(); //  重置扳机标记
            if (_currentRank == RhythmRank.Miss)
                ResetCounter();
            else AddCounter(evt.count);

            //  启用冷却提示条
            if (_comboCount > 0)
            {
                Debug.Log("UIComboInfo 启用冷却提示条");
                bar.StartOrResetCoolDown();
            }
                

            this.ResetTimer(nameof(ResetCounter), 1f);
            text.TextAnimation(_currentRank);

        }
    }
    void OnPlayerAtk(PlayerAtkEvent evt)
    {
        _isTriggered = true;
        this.StartTimer(nameof(ResetTrigger), 0.2f);
       
    }
    void OnRhythmData(RhythmData evt)
    {
        _currentRank = evt.rank;
    }
#endregion

#region 生命周期
    void Start()
    {
        Init();
    }
#endregion
}
