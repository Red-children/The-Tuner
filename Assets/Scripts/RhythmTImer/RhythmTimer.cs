using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  响应BGM调节伤害倍率
public class RhythmTimer : MonoBehaviour
{
    public float[] _multiplierArray = {0.99f, 0.01f};
    public double interval = 0.05f;
    private double _dspStartTime;
    //  TODO:根据歌曲不同调整
    private double _period = 0.5f;
    private int _multiplierIndex = 1;

    // private 
    private void OnPlayBGM(PlayBGMEvent evt)
    {
        //  记录bgm开始时间
        _dspStartTime = evt.time;
    }

    private void OnProgressUpdate(BGMProgressUpdateEvent evt)
    {
        Debug.Log("RhythmTimer: Received BGMProgress");
        //  TODO:检查倍率是否更新
        double offset = evt.PreciseTime % _period;

        int newIndex = offset < interval ? 0:1;
        Debug.Log("newIndex:" + newIndex + "\noffset:" + offset);
        if (newIndex != _multiplierIndex)
        {
            _multiplierIndex = newIndex;
            var newMulitiplierEvent = new AttackMultiplierChangedEvent{
            newMultiplier = _multiplierArray[_multiplierIndex],
            isCritical = newIndex == 0,
            time = AudioSettings.dspTime
            };
        EventBus.Instance.Trigger<AttackMultiplierChangedEvent>(newMulitiplierEvent);
        Debug.Log("Trigger Multipier Changed Event" + _multiplierArray[newIndex]);
        }
    }
    void Start()
    {
        
    }
    void OnEnable()
    {
        PreciseEventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);
        PreciseEventBus.Instance.Subscribe<BGMProgressUpdateEvent>(OnProgressUpdate);
    }
    void Update()
    {
        
    }
}
