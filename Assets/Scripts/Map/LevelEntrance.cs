using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEntrance : MonoBehaviour
{
    [Header("目标点")]
    [Tooltip("玩家会被传送到这个位置")]
    HubManager.ChapterTarget chapterTarget ;

    [Header("关卡音乐")]
    [Tooltip("该关卡播放的BGM")]
    public BGMData levelBGM;

    private void OnTriggerEnter2D(Collider2D other)
    {
        chapterTarget = HubManager.Instance.GetCurrentTarget();
        Transform targetSpawnPoint = GameObject.Find(chapterTarget.spawnPointName).transform ;

        // 只对玩家生效
        if (!other.CompareTag("Player"))
            return;

        if (targetSpawnPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 没有设置目标传送点！");
            return;
        }

        // 直接传送玩家
        other.transform.position = targetSpawnPoint.position;
        
        // 播放关卡音乐
        PlayLevelMusic();
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        chapterTarget = HubManager.Instance.GetCurrentTarget();
        Transform targetSpawnPoint = GameObject.Find(chapterTarget.spawnPointName).transform ;

        // 只对玩家生效
        if (!other.CompareTag("Player"))
            return;

        if (targetSpawnPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 没有设置目标传送点！");
            return;
        }

        // 直接传送玩家
        other.transform.position = targetSpawnPoint.position;
    }
    
    private void PlayLevelMusic()
    {
        // 检查是否有音乐管理器
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            // 这里可以添加播放关卡音乐的代码
            Debug.Log("玩家进入关卡，开始播放关卡音乐");
            // audioManager.PlayLevelMusic(); // 如果AudioManager有播放关卡音乐的方法
        }
        else
        {
            Debug.LogWarning("未找到AudioManager，无法播放关卡音乐");
        }
        
        // 启动节奏管理系统
        StartRhythmSystem();
    }
    
    private void StartRhythmSystem()
    {
        // 查找节奏管理器
        RhythmManager rhythmManager = FindObjectOfType<RhythmManager>();
        if (rhythmManager != null)
        {
            Debug.Log("启动节奏管理器");
            // 这里可以调用节奏管理器的启动方法
            // rhythmManager.StartRhythm();
        }
        else
        {
            Debug.LogWarning("未找到RhythmManager");
        }
        
        // 直接使用传送门配置的BGMData播放音乐
        if (levelBGM != null)
        {
            Debug.Log($"播放关卡BGM: {levelBGM.name}");
            EventBus.Instance.Trigger<PlayBGMEvent>(new PlayBGMEvent(levelBGM));
        }
        else
        {
            Debug.LogWarning("传送门未配置BGMData，无法播放关卡音乐");
        }
        
        Debug.Log("关卡音频系统启动完成");
    }
}
