using System;
using System.Collections;
using UnityEngine;

#region 节奏评级枚举
public enum RhythmRank
{
    Perfect,
    Great,
    Good,
    Miss
}
#endregion

#region 节奏数据结构
public struct RhythmData
{
    public bool isInWindow;
    public RhythmRank rank;
    public double multiplier;

    public RhythmData(bool isInWindow, RhythmRank rank, double multiplier)
    {
        this.isInWindow = isInWindow;
        this.rank = rank;
        this.multiplier = multiplier;
    }
}

#endregion

#region 节奏预览事件
public struct BeatPreviewEvent
{
    public double nextBeatTime;
    public double currentTime;
    public double timeToBeat;
}
#endregion


public class RhythmManager : MonoBehaviour
{

    /// <summary>
    /// 节奏评级配置结构体，包含评级类型、判定窗口（秒）和分数倍率
    /// </summary>
    [Serializable]
    public struct RankConfig
    {
        public RhythmRank rank; //评级类型
        public float window;    //判定窗口（秒），例如0.05表示±50ms
        public float multiplier;//倍率
    }


    public struct RankResult
    {
        public RhythmRank rank;             //当前评级
        public float multiplier;            //当前倍率
        public bool isInWindow;             //是否在任何评级窗口内
        public double judgedDspTime;        //被判定的 DSP 时间
        public double referenceBeatTime;    //用于判定的参考节拍时间    
        public double offsetSeconds;        //偏移时间（秒），正值表示输入在节拍之后，负值表示输入在节拍之前
        public double offsetMilliseconds;   //偏移时间（毫秒），正值表示输入在节拍之后，负值表示输入在节拍之前
    }

    public static RhythmManager Instance { get; private set; }//单例实例

    public int bpm = 120;
    public double previewLead = 0.38;
    public float inputDelayCompensation = 0.05f;

    [Header("Judge Windows")]
    [Tooltip("Default: Perfect=+-50ms, Great=+-100ms, Good=+-150ms")]
    public RankConfig[] rankConfigs = new RankConfig[]
    {
        new RankConfig { rank = RhythmRank.Perfect, window = 0.05f, multiplier = 1.5f },
        new RankConfig { rank = RhythmRank.Great, window = 0.1f, multiplier = 1.2f },
        new RankConfig { rank = RhythmRank.Good, window = 0.15f, multiplier = 1.0f }
    };

    public double BeatProgress { get; private set; }//节拍进度

    private bool isRunning;                             //节奏系统是否正在运行
    private double beatInterval;                        //每拍时间间隔（秒）
    private double firstBeatTime;                       //第一拍的DSP时间
    private double nextBeatTime;                        //下一拍的DSP时间
    private RhythmRank lastRank = RhythmRank.Miss;      //上一次的RhythmRank，用于检测变化
    private Coroutine previewCoroutine;                 //节拍预览协程引用

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RecalculateBeatInterval();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        double now = AudioSettings.dspTime;
        double timeSinceLastBeat = now - (nextBeatTime - beatInterval);
        BeatProgress = beatInterval > 0
            ? Mathf.Clamp01((float)(timeSinceLastBeat / beatInterval))
            : 0f;

        double timeToNext = nextBeatTime - now;
        double absTimeToNext = Math.Abs(timeToNext);
        RhythmRank currentRank = RhythmRank.Miss;
        float currentMultiplier = 0.2f;
        bool inAnyWindow = false;

        //根据距离下一拍的时间，判断当前处于哪个评级窗口内，并设置当前评级和倍率
        foreach (var config in rankConfigs)
        {
            if (absTimeToNext <= config.window)
            {
                currentRank = config.rank;
                currentMultiplier = config.multiplier;
                inAnyWindow = true;
                break;
            }
        }

        if (currentRank != lastRank)
        {
            lastRank = currentRank;//更新上一次的评级
            EventBus.Instance.Trigger(new RhythmData(inAnyWindow, currentRank, currentMultiplier));//发布节奏数据事件，包含是否在窗口内、当前评级和倍率
        }

        if (now >= nextBeatTime)
        {
            nextBeatTime += beatInterval;//当当前时间超过下一拍时间时，更新下一拍时间，确保节奏系统继续运行
        }
    }

    //开始节奏系统，参数为DSP时间的起始点和第一拍的偏移时间
    public void StartRhythm(double dspStartTime, double firstBeatOffset)
    {
        RecalculateBeatInterval();                      //根据当前 BPM 重新计算每拍时间间隔
        firstBeatTime = dspStartTime + firstBeatOffset; //计算第一拍的 DSP 时间
        nextBeatTime = firstBeatTime;                   //初始化下一拍时间为第一拍时间

        double now = AudioSettings.dspTime;             //确保下一拍时间在当前时间之后，避免错过节拍预览
        while (nextBeatTime <= now)
        {
            nextBeatTime += beatInterval;
        }

        isRunning = true;           //标记节奏系统为运行状态
        lastRank = RhythmRank.Miss; 
        ScheduleNextPreview();      //调度第一次节拍预览
    }

    public void StopRhythm()
    {
        isRunning = false;
        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
            previewCoroutine = null;
        }
    }

    public RankResult GetRank()
    {
        return GetRankAtTime(AudioSettings.dspTime);
    }

    public RankResult GetRankAtTime(double dspTime)
    {
        if (!isRunning || beatInterval <= 0)
        {
            return BuildIdleResult(dspTime);
        }

        double compensatedTime = dspTime + inputDelayCompensation;
        return BuildRankResult(compensatedTime, nextBeatTime);
    }

    public RankResult GetDebugRankAtTime(double dspTime)
    {
        if (!isRunning || beatInterval <= 0)
        {
            return BuildIdleResult(dspTime);
        }

        double compensatedTime = dspTime + inputDelayCompensation;
        double referenceBeat = GetNearestBeatTime(compensatedTime);
        return BuildRankResult(compensatedTime, referenceBeat);
    }

    /// <summary>
    /// 精确计算下一次节拍预览的时间，并使用协程在正确的时间触发预览事件
    /// </summary>
    private void ScheduleNextPreview()
    {
        if (!isRunning)
        {
            return;
        }

        double now = AudioSettings.dspTime;
        double waitTime = (nextBeatTime - now) - previewLead;

        if (waitTime > 0)
        {
            previewCoroutine = StartCoroutine(PreviewCoroutine(waitTime));
            return;
        }

        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
        }

        previewCoroutine = StartCoroutine(PreviewImmediateCoroutine());
    }

    private IEnumerator PreviewImmediateCoroutine()
    {
        yield return null;
        TriggerPreview();
    }

    private IEnumerator PreviewCoroutine(double waitTime)
    {
        yield return new WaitForSecondsRealtime((float)waitTime);
        TriggerPreview();
    }

    private void TriggerPreview()
    {
        if (!isRunning)
        {
            return;
        }

        double now = AudioSettings.dspTime;
        //触发节拍预览事件，包含下一拍时间、当前时间和距离下一拍的时间
        EventBus.Instance.Trigger(new BeatPreviewEvent
        {
            nextBeatTime = nextBeatTime,
            currentTime = now,
            timeToBeat = nextBeatTime - now
        });

        //确保下一拍时间在当前时间之后，避免错过节拍预览
        while (nextBeatTime <= now)
        {
            nextBeatTime += beatInterval;
        }
        //进行下一次预览调度
        ScheduleNextPreview();
    }

    private RankResult BuildRankResult(double judgedDspTime, double referenceBeatTime)
    {
        double offsetSeconds = judgedDspTime - referenceBeatTime;
        double absOffset = Math.Abs(offsetSeconds);

        RankResult result = new RankResult
        {
            rank = RhythmRank.Miss,
            multiplier = 0.2f,
            isInWindow = false,
            judgedDspTime = judgedDspTime,
            referenceBeatTime = referenceBeatTime,
            offsetSeconds = offsetSeconds,
            offsetMilliseconds = offsetSeconds * 1000.0
        };

        foreach (var config in rankConfigs)
        {
            if (absOffset <= config.window)
            {
                result.rank = config.rank;
                result.multiplier = config.multiplier;
                result.isInWindow = true;
                break;
            }
        }

        return result;
    }

    private RankResult BuildIdleResult(double dspTime)
    {
        return new RankResult
        {
            rank = RhythmRank.Miss,
            multiplier = 1f,
            isInWindow = false,
            judgedDspTime = dspTime,
            referenceBeatTime = dspTime,
            offsetSeconds = 0,
            offsetMilliseconds = 0
        };
    }

    /// <summary>
    /// 得到最近的节拍节点
    /// </summary>
    /// <param name="dspTime"></param>
    /// <returns></returns>
    private double GetNearestBeatTime(double dspTime)
    {
        if (beatInterval <= 0)
        {
            return nextBeatTime;
        }

        if (dspTime <= firstBeatTime)
        {
            return firstBeatTime;
        }

        //计算公式   (当前时间 - 第一拍时间)/每拍时间间隔， 后面的意思是四舍五入 得到的是当前节拍的索引 
        double beatIndex = Math.Round((dspTime - firstBeatTime) / beatInterval, MidpointRounding.AwayFromZero);

        //根据索引计算出最近的节拍时间
        return firstBeatTime + beatIndex * beatInterval;
    }

    /// <summary>
    /// 计算每拍时间间隔，单位为秒。根据当前 BPM 进行计算，如果 BPM 小于等于 0，则使用默认值 0.5 秒（120 BPM 的节拍间隔）。当 BPM 发生变化时，调用此方法重新计算节拍间隔。
    /// </summary>
    private void RecalculateBeatInterval()
    {
        beatInterval = bpm > 0 ? 60.0 / bpm : 0.5;
    }
}
