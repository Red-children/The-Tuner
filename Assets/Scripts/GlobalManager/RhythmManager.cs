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
    public static RhythmManager Instance { get; private set; }

    public int bpm = 120;           //歌曲bpm 后续可以改成从音乐文件中读取

    public double previewLead = 0.2;          // 指示器提前量（秒），可在Inspector调整
    private bool previewTriggeredForNextBeat = false; // 防止同一拍多次触发预告


    private double beatInterval;      // 每拍时长
    private double nextBeatTime;      // 下一拍的时间
    public bool isInWindow;
    
    private RhythmRank lastRank; // 记录上一帧的判定等级，用于检测变化
    private RhythmRank currentRank;

    #region 节拍变化数据库 用于调整节拍的各项数据 

    [System.Serializable]
    public struct RankConfig
    {
        public RhythmRank rank;     // 判定等级
        public float window;        // 判定窗口半宽（秒），例如 Perfect 为 0.03f
        public float multiplier;    // 对应的伤害倍率，例如 Perfect 为 2.0f
    }

    public RankConfig[] rankConfigs; // 在 Inspector 中配置，按优先级从高到低排列

    #endregion

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
        beatInterval = 60.0 / bpm;    //计算每拍的时间间隔
        nextBeatTime = AudioSettings.dspTime + beatInterval; // 初始化下一拍时间为当前时间加一个拍的间隔
        lastRank = RhythmRank.Miss; // 初始状态为 Miss
    }

    void Update()
    {
        double now = AudioSettings.dspTime;                     // 获取当前时间
        double timeToNext = nextBeatTime - now;                 // 计算距离下一拍的时间差
        double absTimeToNext = Mathf.Abs((float)timeToNext);    // 绝对值用于比较窗口大小

        // --- 新增：节拍预告逻辑 ---
        if (!previewTriggeredForNextBeat && timeToNext <= previewLead && timeToNext > 0)
        {
            previewTriggeredForNextBeat = true;
            // 发布预告事件
            EventBus.Instance.Trigger(new BeatPreviewEvent
            {
                nextBeatTime = nextBeatTime,
                currentTime = now,
                timeToBeat = timeToNext
            });
            Debug.Log($"[RhythmManager] 预告下一拍 | 时间差：{timeToNext:F4}");
        }



        // 默认是 Miss
        currentRank = RhythmRank.Miss;               
        float currentMultiplier = 0.2f; // Miss 倍率 Miss的倍率就不在数据库内可见了
        bool inAnyWindow = false;

        #region 判定等级检索
        // 按优先级从高到低检查 利用break确保只匹配到最高优先级的窗口 但是需要把高优先级的窗口放在前面
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
        #endregion

        #region 触发事件 只有当等级发生变化时才触发事件，避免重复触发同一等级的事件（lastRank专门为此设置）
        // 如果等级变化，触发事件
        if (currentRank != lastRank)
        {
            lastRank = currentRank;  // 更新上一帧的等级
            EventBus.Instance.Trigger(new RhythmData(inAnyWindow, currentRank, currentMultiplier));
            Debug.Log($"[RhythmManager] Rank: {currentRank}, Multiplier: {currentMultiplier}");
        }
        #endregion

        #region 更新下一拍时间
        // 如果已经过了拍点，更新下一拍
        if (now >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
        }
        #endregion

    }
}