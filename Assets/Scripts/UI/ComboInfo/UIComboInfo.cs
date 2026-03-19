using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComboInfo : MonoBehaviour
{
    //  冷却条
    public UIComboInfoBar bar;
    //  文本
    public UIComboInfoText text;

    private int _comboCount = 0;
    private bool _isTriggered = false;

    private RhythmRank _currentRank;
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

        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
        EventBus.Instance.Subscribe<PlayerAtkEvent>(OnPlayerAtk);
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
    }
#region 回调函数
    void OnEnemyHit(EnemyHitEvent evt)
    {
        if (_isTriggered)
        {
            switch(_currentRank)
            {
                case RhythmRank.Miss:
                    _comboCount = 0;
                    break;
                case RhythmRank.Good:
                    break;
                case RhythmRank.Great:
                    break;
                case RhythmRank.Perfect:
                    break;
                default:    break; 
            }
        }
    }
    void OnPlayerAtk(PlayerAtkEvent evt)
    {
        _isTriggered = true;
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
