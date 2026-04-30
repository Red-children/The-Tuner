using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class PlayerSpawnInfo
{
    public static string spawnPointName;
}
public class HubManager : MonoBehaviour
{
    public static HubManager Instance { get; private set; }

    // 章节目标数据结构
    [Serializable]
    public class ChapterTarget
    {
        public string sceneName;      // 目标场景名
        public string spawnPointName; // 玩家在目标场景的出生点名称
    }

    [Header("章节配置")]
    public List<ChapterTarget> chapters = new List<ChapterTarget>();

    [Header("剧情进度")]
    public int currentChapter = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 跨场景保留
    }

    /// <summary>
    /// 获取当前章节的目标场景信息
    /// </summary>
    public ChapterTarget GetCurrentTarget()
    {
        if (currentChapter >= 0 && currentChapter < chapters.Count)
            return chapters[currentChapter];
        return null;
    }

    /// <summary>
    /// 完成当前章节，推进到下一章
    /// </summary>
    public void CompleteChapter()
    {
        currentChapter++;
        // 这里可以触发对话之类
    }


    
}

