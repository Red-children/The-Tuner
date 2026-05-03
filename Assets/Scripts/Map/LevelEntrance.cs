using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class LevelEntrance : MonoBehaviour
{
    [Header("目标点")]
    [Tooltip("玩家会被传送到这个位置")]
    
    HubManager.ChapterTarget chapterTarget ;

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
}
