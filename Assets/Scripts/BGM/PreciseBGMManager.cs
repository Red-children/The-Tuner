using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
public class PreciseBGMManager : MonoBehaviour
{
    //  歌曲设定
    //  TODO:根据歌曲不同调整
    [Header("伤害倍率")]
    public float[] _multiplierArray = {0.99f, 0.01f};
    [Header("精准区间")]
    public double windowPeriod = 0.2f;
    [Header("1s/BPM")]
    [Header("周期")]
    private double _period = 0.5f;
    [Header("指示器设置")]
    public double lead = 0.02f; //  指示器提前量
    private double _pointToCome = 0; //  下一个关键点
    private bool _isCurrentPointPublished = true;

    private int _multiplierIndex = 1;


    [SerializeField] private AudioSource _bgmAudioSource;
    [SerializeField] private AudioClip _bgmClip;
    [Tooltip("采样间隔(毫秒),建议10ms(100Hz)兼顾精度和性能")]
    [Range(1, 20)] public int sampleIntervalMs = 10;

/************/
    // 音频时钟（DSP Time）：和音频硬件同步，无帧延迟
    [Header("歌曲进程信息")]
    private double _dspStartTime; // BGM开始播放时的DSP时间
    private float _preciseTime;   //  已播放时间
    private bool _isPlaying = false;

#region 指示器
    private class IndicatorState
    {
        public double NextPoint { get; set; }
        public double PublishTime { get; set; }
        public bool IsPointExpired { get; set; }
        public bool IsPublishTimeReached { get; set; }
    }
    // 1. 纯过滤：判断是否需要执行后续逻辑
    private bool IsIndicatorEventValid()
    {
        if (_preciseTime < 0.4) return false;
        if (_pointToCome > _bgmClip.length) return false;
        return true;
    }
    // 2. 纯计算：只算不判断，返回封装的状态
    private IndicatorState CalculateIndicatorState()
    {
        double publishTime = _pointToCome - (lead + windowPeriod);
        return new IndicatorState
        {
            NextPoint = _pointToCome,
            PublishTime = publishTime,
            IsPointExpired = _preciseTime > _pointToCome,
            IsPublishTimeReached = _preciseTime >= publishTime
        };
    }
    // 3. 纯逻辑：仅处理标记更新
    private void UpdateIndicatorFlag(IndicatorState state)
    {
        if (!_isCurrentPointPublished) return;
        
        // 用布尔运算消除嵌套，直接赋值
        _isCurrentPointPublished = !(state.IsPointExpired || state.IsPublishTimeReached);
        
        // 未到发布时间则终止执行
        if (!state.IsPublishTimeReached)
        {
            // Debug.Log($"未到发布时间 | 当前时间:{_preciseTime:F4} | 发布时间:{state.PublishTime:F4}");
            return;
        }
    }
    // 4. 纯执行：仅发布事件
    private void PublishIndicatorEvent(double nextPoint)
    {
        var evt = new IndicatorActiveEvent
        {
            nextPoint = nextPoint,
            time = _preciseTime
        };
        PreciseEventBus.Instance.Trigger<IndicatorActiveEvent>(evt);
        Debug.Log($"发布指示器 | NextPoint:{nextPoint:F8} | 发布时间:{_preciseTime:F8}");
    }


    private void PushIndicatorEvent()
    {
        //  前置过滤
        if (!IsIndicatorEventValid()) return;

        //  计算核心状态
        IndicatorState state = CalculateIndicatorState();

        //  处理标记
        UpdateIndicatorFlag(state);

        //  发布事件
        if (state.IsPublishTimeReached)
        {
            if (!_isCurrentPointPublished)
            {
                PublishIndicatorEvent(state.NextPoint);
                _pointToCome += _period;
                _isCurrentPointPublished = true;
            }
        }
        else
        {
            if(_preciseTime > state.NextPoint)
            {
                _isCurrentPointPublished = false;
            }
        }


        // Debug.Log($"PushIndicatorEvent | PreciseTime:{_preciseTime:F4} | NextPoint:{state.NextPoint:F4}");
    }
#endregion 指示器

    private void PushMutiplierChangeEvent()
    {
        //  TODO:检查倍率是否更新
        double offset = _preciseTime % _period;
        double ahead = _period - offset;
        int newIndex = ahead < windowPeriod ? 0:1;
        Debug.Log("newIndex:" + newIndex + "\noffset:" + offset);
        //  发布倍率变动
        if (newIndex != _multiplierIndex)
        {
            _multiplierIndex = newIndex;
            var newMulitiplierEvent = new AttackMultiplierChangedEvent{
                newMultiplier = _multiplierArray[_multiplierIndex],
                isCritical = newIndex == 0,
                time = AudioSettings.dspTime
            };
        EventBus.Instance.Trigger<AttackMultiplierChangedEvent>(newMulitiplierEvent);
        // Debug.Log("Trigger Multipier Changed Event" + _multiplierArray[newIndex]);
        Debug.Log("PushMutiplierChangeEvent\nPreciseTime:"+ _preciseTime);
        }
    }

    private void PushBGMProgressEvent()
    {
        // 核心：用DSP时间计算精准进度（无帧延迟）
        double currentDspTime = AudioSettings.dspTime;
        _preciseTime = (float)(currentDspTime - _dspStartTime);
        // 防止进度超过总时长（音频结束前的最后一帧）
        _preciseTime = Mathf.Clamp(_preciseTime, 0, _bgmClip.length);
        // 计算进度数据
        float progress = _preciseTime / _bgmClip.length;

        // 推送精准进度事件（复用之前的事件类）
        BGMProgressUpdateEvent progressEvent = new BGMProgressUpdateEvent
        {
            Progress = progress,
            PreciseTime = _preciseTime,
            TotalDuration = _bgmClip.length
        };
        PreciseEventBus.Instance.Trigger(progressEvent);
        Debug.Log("PushBGMProgressEvent\nPreciseTime:" + _preciseTime);
    }

    private void OnPlayBGM(PlayBGMEvent evt)
    {
        Debug.Log("BGMTest:BGMManger Received");
        if (_bgmAudioSource.isPlaying)
        {
            Debug.Log("BGMTest isPlaying");
            return;
        }
        // 记录DSP开始时间（音频硬件的时间，比Time.time精准）
        _dspStartTime = AudioSettings.dspTime;
        _pointToCome = _period;                      //  关键点时间
        _bgmAudioSource.PlayScheduled(_dspStartTime); // 精准调度播放
        _isPlaying = true;
        _isCurrentPointPublished = true;
        // 启动高频采样协程（替代Update，减少空判断）
        StartCoroutine(PreciseProgressSampler());

    }
    private IEnumerator PreciseProgressSampler()
    {
        // 转换采样间隔为秒
        float interval = sampleIntervalMs / 1000f;
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(interval); // 不受Time.timeScale影响

        while (_isPlaying && _bgmAudioSource.isPlaying)
        {
            PushBGMProgressEvent();
            PushMutiplierChangeEvent();
            //  歌曲末尾跳出
            PushIndicatorEvent();
            yield return wait;
        }

        _isPlaying = false;
    }

    private void InitializeManager()
    {
        // 关键配置：音游BGM必须用Streaming加载，且禁用空间音效
        _bgmAudioSource.loop = false;
        _bgmAudioSource.spatialBlend = 0;
        _bgmAudioSource.playOnAwake = false;
        _bgmAudioSource.clip = _bgmClip;
        _bgmAudioSource.volume = 1f;

        // 订阅播放事件
        PreciseEventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);
    }

    public void StopBGM()
    {
        _isPlaying = false;
        _bgmAudioSource.Stop();
    }

    private void OnDestroy()
    {
        // 取消订阅，清理资源
        PreciseEventBus.Instance.Unsubscribe<PlayBGMEvent>(OnPlayBGM);
        _isPlaying = false;
        StopCoroutine(PreciseProgressSampler());
    }
    void Awake()
    {
        InitializeManager();
    }
}
