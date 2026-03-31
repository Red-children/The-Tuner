using System;
using System.Collections;
using UnityEngine;

public enum RhythmRank
{
    Perfect,
    Great,
    Good,
    Miss
}

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

public struct BeatPreviewEvent
{
    public double nextBeatTime;
    public double currentTime;
    public double timeToBeat;
}

public class RhythmManager : MonoBehaviour
{
    [Serializable]
    public struct RankConfig
    {
        public RhythmRank rank;
        public float window;
        public float multiplier;
    }

    public struct RankResult
    {
        public RhythmRank rank;
        public float multiplier;
        public bool isInWindow;
        public double judgedDspTime;
        public double referenceBeatTime;
        public double offsetSeconds;
        public double offsetMilliseconds;
    }

    public static RhythmManager Instance { get; private set; }

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

    public double BeatProgress { get; private set; }

    private bool isRunning;
    private double beatInterval;
    private double firstBeatTime;
    private double nextBeatTime;
    private RhythmRank lastRank = RhythmRank.Miss;
    private Coroutine previewCoroutine;

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
            lastRank = currentRank;
            EventBus.Instance.Trigger(new RhythmData(inAnyWindow, currentRank, currentMultiplier));
        }

        if (now >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
        }
    }

    public void StartRhythm(double dspStartTime, double firstBeatOffset)
    {
        RecalculateBeatInterval();
        firstBeatTime = dspStartTime + firstBeatOffset;
        nextBeatTime = firstBeatTime;

        double now = AudioSettings.dspTime;
        while (nextBeatTime <= now)
        {
            nextBeatTime += beatInterval;
        }

        isRunning = true;
        lastRank = RhythmRank.Miss;
        ScheduleNextPreview();
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

    public float GetCurrentMultiplier()
    {
        return GetRank().multiplier;
    }

    public double GetNextBeatTimeWithCompensation()
    {
        return nextBeatTime - inputDelayCompensation;
    }

    public double GetNextBeatTime()
    {
        return nextBeatTime;
    }

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
        EventBus.Instance.Trigger(new BeatPreviewEvent
        {
            nextBeatTime = nextBeatTime,
            currentTime = now,
            timeToBeat = nextBeatTime - now
        });

        while (nextBeatTime <= now)
        {
            nextBeatTime += beatInterval;
        }

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

        double beatIndex = Math.Round((dspTime - firstBeatTime) / beatInterval, MidpointRounding.AwayFromZero);
        return firstBeatTime + beatIndex * beatInterval;
    }

    private void RecalculateBeatInterval()
    {
        beatInterval = bpm > 0 ? 60.0 / bpm : 0.5;
    }
}
