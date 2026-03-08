using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngineInternal;

public enum RhythmRank
{
    Perfect,    // 完美
    Great,      // 优秀
    Good,       // 良好
    Miss        // 失误（窗口外）
}

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


public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance { get; private set; }

    public int bpm = 120;
    public float windowSize = 0.1f; // 判定窗口大小（秒）

    private double beatInterval;      // 每拍时长
    private double nextBeatTime;      // 下一拍的时间
    public bool isInWindow;
    public float currentMultiplier = 1f;

    private RhythmRank lastRank; // 记录上一帧的判定等级，用于检测变化

    [System.Serializable]
    public struct RankConfig
    {
        public RhythmRank rank;
        public float window;      // 判定窗口半宽（秒），例如 Perfect 为 0.03f
        public float multiplier;
    }

    public RankConfig[] rankConfigs; // 在 Inspector 中配置，按优先级从高到低排列

    // [RuntimeInitializeOnLoadMethod]
    //private static void AutoCreate()
    //{
    //    if (Instance == null)
    //    {
    //        GameObject go = new GameObject("RhythmManager");
    //        Instance = go.AddComponent<RhythmManager>();
    //        DontDestroyOnLoad(go);
    //    }
    //}

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 可选，根据需要
    }

    void Start()
    {
        beatInterval = 60.0 / bpm;
        nextBeatTime = AudioSettings.dspTime + beatInterval;
        lastRank = RhythmRank.Miss; // 初始状态为 Miss
    }

    void Update()
    {
        double now = AudioSettings.dspTime;
        double timeToNext = nextBeatTime - now;
        double absTimeToNext = Mathf.Abs((float)timeToNext);

        // 默认是 Miss
        RhythmRank currentRank = RhythmRank.Miss;
        float currentMultiplier = 0.2f; // Miss 倍率
        bool inAnyWindow = false;

        // 按优先级从高到低检查
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

        // 如果等级变化，触发事件
        if (currentRank != lastRank)
        {
            lastRank = currentRank;
            EventBus.Instance.Trigger(new RhythmData(inAnyWindow, currentRank, currentMultiplier));
            Debug.Log($"[RhythmManager] Rank: {currentRank}, Multiplier: {currentMultiplier}");
        }

        // 如果已经过了拍点，更新下一拍
        if (now >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
        }
    }
}