using System;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    public static HubManager Instance { get; private set; }

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

    [Header("传送目标")]
    [Tooltip("对话结束后传送玩家到这个位置")]
    public Transform teleportTarget;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        UIManager.Instance.OpenPanel(UIManager.UIConst.Battle);
        UIManager.Instance.OpenPanel(UIManager.UIConst.Crosshair);
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

    /// <summary>
    /// 传送玩家到目标位置
    /// </summary>
    public void TeleportToTarget()
    {
        if (teleportTarget == null)
        {
            Debug.LogWarning("HubManager: 传送目标位置未设置");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("HubManager: 未找到玩家对象");
            return;
        }

        // 打开加载界面
        UIManager.Instance.OpenPanel(UIManager.UIConst.Loading);

        // 执行传送
        player.transform.position = teleportTarget.position;
        player.transform.rotation = teleportTarget.rotation;

        Debug.Log($"玩家已传送到目标位置: {teleportTarget.position}");

        // 延迟关闭加载界面（2秒后自动关闭）
        Invoke(nameof(CloseLoadingPanel), 2f);
    }

    private void CloseLoadingPanel()
    {
        UIManager.Instance.ClosePanel(UIManager.UIConst.Loading);
    }
}