using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerSpawnInfo
{
    public static string spawnPointName;
}

public class HubManager : MonoBehaviour
{
    public static HubManager Instance { get; private set; }//

    [Serializable]
    public class ChapterTarget
    {
        public string sceneName;
        public string spawnPointName;
    }

    [Header("章节配置")]
    public List<ChapterTarget> chapters = new List<ChapterTarget>();

    [Header("剧情进度")]
    public int currentChapter = 0;

    [Header("主脑对话状态")]
    public bool hasTalkedToHub = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ChapterTarget GetCurrentTarget()
    {
        if (currentChapter >= 0 && currentChapter < chapters.Count)
            return chapters[currentChapter];
        return null;
    }

    public void CompleteChapter()
    {
        currentChapter++;
    }
}

