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
    //  精准区间
    public double windowPeriod = 0.2f;
    [Header("周期")]
    //  BPM
    public double BPM = 120;
    //  前firstOffset不呼叫指示器
    public double firstOffset = 1.1f;

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
    private Queue<double> _beatTimestampQueue; // 关键音符（节拍）时间戳队列
    private Queue<double> _indicatorPublishQueue; // Indicator推送时间戳队列

#region 指示器
    [Header("指示器设置")]
    public double lead = 0.2f; //  指示器提前量
    private double _pointToCome = 0; //  下一个关键点
    private bool _hasPublishedCurrentPoint  = true;

    private int _multiplierIndex = 1;
    private class IndicatorState
    {
        public double NextPoint { get; set; }
        public double PublishTime { get; set; }
        public bool IsPointExpired { get; set; }
        public bool IsPublishTimeReached { get; set; }
    }
    // 1. 纯过滤：判断是否需要执行后续逻辑
    // private bool IsIndicatorEventValid()
    // {
    //     //  TODO:
    //     return false;
    // }

    // 3. 纯逻辑：仅处理标记更新
    // private void UpdateIndicatorFlag(IndicatorState state)
    // {
    //     // 关键点过期：标记为已发布（避免重复发布）
    //     if (state.IsPointExpired)
    //     {
    //         _hasPublishedCurrentPoint = true;
    //         return;
    //     }

    //     // 到达发布时间但未发布：标记为待发布
    //     if (state.IsPublishTimeReached && !_hasPublishedCurrentPoint)
    //     {
    //         _hasPublishedCurrentPoint = true; // 标记为已发布，避免重复
    //     }
    // }
    // 4. 纯执行：仅发布事件
    // private void PublishIndicatorEvent(double nextPoint)
    // {
    //     var evt = new IndicatorActiveEvent
    //     {
    //         nextPoint = nextPoint,
    //         time = _preciseTime
    //     };
    //     PreciseEventBus.Instance.Trigger<IndicatorActiveEvent>(evt);
    //     Debug.Log($"发布指示器 | NextPoint:{nextPoint:F8} | 发布时间:{_preciseTime:F8}");
    // }


    private void PushIndicatorEvent()
    {
        Debug.Log($"PushIndicatorEvent: _preciseTime={_preciseTime}, 队列长度={_indicatorPublishQueue?.Count ?? 0}");
        if (_preciseTime < 1.1f || _indicatorPublishQueue.Count == 0)
            return;

        // 查看队列首个待推送时间
        double targetPublishTime = _indicatorPublishQueue.Peek();

        // 时间匹配：到达推送时间 → 出队并发布事件
        if (targetPublishTime - _preciseTime < 1e-3)
        {
            // 出队
            double publishTime = _indicatorPublishQueue.Dequeue();
            double currentBeatTime = _beatTimestampQueue.Dequeue();

            // 发布事件
            var evt = new IndicatorActiveEvent
            {
                BPM = BPM,
                time = _preciseTime,
                nextPoint = currentBeatTime
            };
            PreciseEventBus.Instance.Trigger<IndicatorActiveEvent>(evt);
            Debug.Log($"推送Indicator | 节拍时间：{currentBeatTime:F8} | 实际推送时间：{_preciseTime:F8} | 目标推送时间：{publishTime:F8}");
        }
    }
#endregion 指示器

    private void PreGenerateTimestamps()
    {
        _beatTimestampQueue = new Queue<double>();
        _indicatorPublishQueue = new Queue<double>();

        // 计算单个节拍的Indicator推送提前量（逻辑不变）
        double judgeWindowOffset = windowPeriod;
        double totalPublishAdvance = judgeWindowOffset + lead;

        // 生成所有时间戳并加入队列
        double currentBeatTime = 60 / BPM;
        while (currentBeatTime < _bgmClip.length)
        {
            _beatTimestampQueue.Enqueue(currentBeatTime); // 入队节拍时间
            double publishTime = currentBeatTime - totalPublishAdvance;
            _indicatorPublishQueue.Enqueue(publishTime); // 入队推送时间

            currentBeatTime += 60 / BPM;
        }

        Debug.Log($"预生成完成 | 总节拍数：{_beatTimestampQueue.Count} | 第一个推送时间：{_indicatorPublishQueue.Peek():F8}");
    }

    private void PushMutiplierChangeEvent()
    {
        //  TODO:检查倍率是否更新
        double offset = _preciseTime % (60 / BPM);  //  拍间进行时间
        double ahead = 60 / BPM - offset;           //  距离下一拍时间
        int newIndex = ahead < windowPeriod ? 0:1;
        // Debug.Log("newIndex:" + newIndex + "\noffset:" + offset);
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
        Debug.Log("PushMutiplierChangeEvent\nPreciseTime:"+ _preciseTime);
        }
    }

    private void PushBGMProgressEvent()
    {
        // 核心：用DSP时间计算精准进度
        double currentDspTime = AudioSettings.dspTime;
        _preciseTime = (float)(currentDspTime - _dspStartTime);
        // 防止进度超过总时长
        _preciseTime = Mathf.Clamp(_preciseTime, 0, _bgmClip.length);
        // 计算进度数据
        float progress = _preciseTime / _bgmClip.length;

        // 推送精准进度事件
        BGMProgressUpdateEvent progressEvent = new BGMProgressUpdateEvent
        {
            Progress = progress,
            PreciseTime = _preciseTime,
            TotalDuration = _bgmClip.length
        };
        PreciseEventBus.Instance.Trigger(progressEvent);
        // Debug.Log("PushBGMProgressEvent\nPreciseTime:" + _preciseTime);
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
        PreGenerateTimestamps();    //  生成关键时间戳序列
        _dspStartTime = AudioSettings.dspTime;
        Debug.Log($"记录DSP开始时间{_dspStartTime}");
        _bgmAudioSource.PlayScheduled(_dspStartTime); // 精准调度播放
        _isPlaying = true;
        _hasPublishedCurrentPoint = false;
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
        // StopCoroutine(PreciseProgressSampler());
        StopAllCoroutines();
    }
    void Awake()
    {
        InitializeManager();
    }
}
