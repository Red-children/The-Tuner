using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngineInternal;

#region 节拍结构枚举 
public enum RhythmRank
{
    Perfect,    // 完美
    Great,      // 优秀
    Good,       // 良好
    Miss        // 失误（窗口外）
}
#endregion

#region 节拍数据结构专门用于事件传递
public struct RhythmData
{
    public bool isInWindow;      // 是否在任意窗口内（可选，方便快速判断）
    public RhythmRank rank;      // 当前判定等级
    public double multiplier;    // 对应的伤害倍率

    public RhythmData(bool isInWindow, RhythmRank rank, double multiplier)
    {
        this.isInWindow = isInWindow;
        this.rank = rank;
        this.multiplier = multiplier;
    }
}
#endregion

public struct BeatPreviewEvent
{
    public double nextBeatTime;      // 下一个节拍的时间（绝对时间）
    public double currentTime;       // 当前音乐时间
    public double timeToBeat;        // 距离下一拍的时间（正数）
}


public class RhythmManager : MonoBehaviour
{
    private bool isRunning = false;
    public static RhythmManager Instance { get; private set; }

    public int bpm = 120;
    public double previewLead = 0.38; // 可在Inspector调整

    private double beatInterval;
    private double nextBeatTime;
    public double BeatProgress { get; private set; }

    private RhythmRank lastRank;
    private Coroutine previewCoroutine;

    [System.Serializable]
    public struct RankConfig
    {
        public RhythmRank rank;
        public float window;
        public float multiplier;
    }
    public RankConfig[] rankConfigs;

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

    void Start()
    {
        beatInterval = 60.0 / bpm;
        lastRank = RhythmRank.Miss;
    }

    void Update()
    {
        if (!isRunning) return;

        double now = AudioSettings.dspTime;

        // 更新 BeatProgress（用于视觉缩放）
        double timeSinceLastBeat = now - (nextBeatTime - beatInterval);
        BeatProgress = Mathf.Clamp01((float)(timeSinceLastBeat / beatInterval));

        // 计算当前等级（用于发布 RhythmData 事件）
        double timeToNext = nextBeatTime - now;
        double absTimeToNext = Mathf.Abs((float)timeToNext);
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
            Debug.Log($"[RhythmManager] Rank: {currentRank}, Multiplier: {currentMultiplier}");
        }

        // 如果时间已过拍点，推进到下一拍（协程可能已经做了，但这里作为备份）
        if (now >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
        }
    }

    public void StartRhythm(double dspStartTime, double firstBeatOffset)
    {
        nextBeatTime = dspStartTime + firstBeatOffset;
        double now = AudioSettings.dspTime;
        while (nextBeatTime <= now)
        {
            nextBeatTime += beatInterval;
        }
        isRunning = true;
        lastRank = RhythmRank.Miss;
        ScheduleNextPreview(); // 启动预告调度
        Debug.Log($"[RhythmManager] 已启动，下一拍时间 {nextBeatTime:F8}");
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

    private void ScheduleNextPreview()
    {
        if (!isRunning) return;
        double now = AudioSettings.dspTime;
        double timeToNext = nextBeatTime - now;
        double waitTime = timeToNext - previewLead;

        if (waitTime > 0)
        {
            previewCoroutine = StartCoroutine(PreviewCoroutine(waitTime));
        }
        else
        {
            // 已经过了预告时间，但为了避免同一帧无限递归，等待一帧再触发
            if (previewCoroutine != null) StopCoroutine(previewCoroutine);
            previewCoroutine = StartCoroutine(PreviewImmediateCoroutine());
        }
    }

    private IEnumerator PreviewImmediateCoroutine()
    {
        yield return null; // 等待一帧
        TriggerPreview();
    }
    #region 预告协程
    private IEnumerator PreviewCoroutine(double waitTime)
    {
        yield return new WaitForSecondsRealtime((float)waitTime);
        TriggerPreview();
    }
    #endregion

    #region 触发预告
    private void TriggerPreview()
    {
        if (!isRunning) return;
        double now = AudioSettings.dspTime;
        EventBus.Instance.Trigger(new BeatPreviewEvent
        {
            nextBeatTime = nextBeatTime,
            currentTime = now,
            timeToBeat = nextBeatTime - now
        });

        // 推进下一拍（可能已经过了拍点，需要循环推进到未来）
        while (nextBeatTime <= now)
        {
            nextBeatTime += beatInterval;
        }
        ScheduleNextPreview();
    }
    #endregion

    // 实时判定方法
    public struct RankResult
    {
        public RhythmRank rank;
        public float multiplier;
        public bool isInWindow;
    }

    public RankResult GetRank(double dspTime)
    {
        double timeToNext = nextBeatTime - dspTime;
        double absTimeToNext = Mathf.Abs((float)timeToNext);

        RankResult result = new RankResult { rank = RhythmRank.Miss, multiplier = 0.2f, isInWindow = false };
        foreach (var config in rankConfigs)
        {
            if (absTimeToNext <= config.window)
            {
                result.rank = config.rank;
                result.multiplier = config.multiplier;
                result.isInWindow = true;
                break;
            }
        }
        return result;
    }

    public double GetNextBeatTime() => nextBeatTime;
}