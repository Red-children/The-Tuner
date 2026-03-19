using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("房间配置")]
    public Collider2D roomTrigger;          // 门口触发器（通常是一个大区域）
    public WaveManager waveManager;          // 本房间的波次管理器
    public AudioClip bgmClip;                // 本房间的背景音乐（可选）
    public Door[] doors;                     // 房间的门（数组，支持多个门）

    public List<FSM> enemiesInRoom = new List<FSM>();
    private bool isCleared = false;          // 是否已通关
    public LayerMask obstacleMask;           // 障碍物层（墙壁、装饰等）


    private void Awake()
    {
        if (roomTrigger == null)
            roomTrigger = GetComponent<Collider2D>();
        if (waveManager == null)
            waveManager = GetComponent<WaveManager>();
    }

    // 由波次管理器调用，注册敌人
    public void RegisterEnemy(FSM enemy)
    {
        if (!enemiesInRoom.Contains(enemy))
            enemiesInRoom.Add(enemy);
    }

    // 敌人死亡时调用，注销
    public void UnregisterEnemy(FSM enemy)
    {
        enemiesInRoom.Remove(enemy);
        if (enemiesInRoom.Count == 0 && !isCleared)
        {
            isCleared = true;
            OnRoomCleared();
        }
    }

    private void OnRoomCleared()
    {
        // 打开所有门
        foreach (var door in doors)
        {
            if (door != null)
                door.Open();
        }
        Debug.Log("房间已清空，门已打开");
        // 可触发宝箱生成、播放音效等
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 关闭所有门
            foreach (var door in doors)
            {
                if (door != null)
                    door.Close();
            }

            // 激活所有敌人
            foreach (var enemy in enemiesInRoom)
            {
                if (enemy != null)
                    enemy.parameter.target = other.transform;
            }

            // 启动波次（如果还没启动）
            if (waveManager != null && !waveManager.isWaveActive)
                waveManager.StartWave(this);

            // 禁用入口触发器，防止重复触发
            if (roomTrigger != null)
                roomTrigger.enabled = false;
        }
    }

    //用碰撞检测来判断玩家是否进入房间
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var enemy in enemiesInRoom)
            {
                if (enemy != null)
                    enemy.parameter.target = null;
            }
        }
    }

    public Vector2 GetRandomValidPoint(float safeRadius = 0.5f)
    {
        if (roomTrigger == null) return transform.position;

        Bounds bounds = roomTrigger.bounds;
        int maxAttempts = 50;

        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 point = new Vector2(x, y);

            if (!roomTrigger.OverlapPoint(point)) continue;

            // 检查周围 safeRadius 半径内是否有障碍物
            Collider2D[] hits = Physics2D.OverlapCircleAll(point, safeRadius, obstacleMask);
            if (hits.Length > 0) continue;

            return point;
        }

        
        // 如果没有预设点，返回房间中心（但建议确保中心安全）
        Debug.LogWarning("未找到安全生成点，返回房间中心，请检查房间障碍物或增加预设点");
        return bounds.center;
    }
}