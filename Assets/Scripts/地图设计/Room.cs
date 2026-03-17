using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("房间配置")]
    public Collider2D roomTrigger;          // 门口触发器（通常是一个大区域）
    public WaveManager waveManager;          // 本房间的波次管理器
    public AudioClip bgmClip;                // 本房间的背景音乐（可选）

    private List<FSM> enemiesInRoom = new List<FSM>();
    private bool isCleared = false;          // 是否已通关

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
        // 可触发宝箱生成、开门、播放音效等
        Debug.Log("房间已清空");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 激活所有敌人
            foreach (var enemy in enemiesInRoom)
            {
                if (enemy != null)
                    enemy.parameter.target = other.transform;
            }

            // 启动波次（如果还没启动）
            if (waveManager != null && !waveManager.isWaveActive)
                waveManager.StartWave(this);

            //// 切换音乐（可选）
            //if (bgmClip != null)
            //    EventBus.Instance.Trigger(new ChangeBgmEvent { clip = bgmClip });
        }
    }

    //用碰撞检测来判断玩家是否进入房间
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 玩家离开，清空敌人目标（让它们恢复巡逻）
            foreach (var enemy in enemiesInRoom)
            {
                if (enemy != null)
                    enemy.parameter.target = null;
            }

            // 停止波次？通常不停止，但可根据需要暂停
        }
    }
}