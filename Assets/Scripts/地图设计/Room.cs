using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("房间配置")]
    public Collider2D roomTrigger;          // 入口触发器（用于检测玩家进入）
    public WaveManager waveManager;
    public Door[] doors;
    public LayerMask obstacleMask;

    private Bounds cachedBounds;             // 缓存房间范围（用于敌人生成）
    private bool isActive = false;           // 房间是否已被激活
    private bool isCleared = false;
    private List<EnemyController> enemiesInRoom = new List<EnemyController>();

    public void Init(RoomType roomType)
    {
        
    }



    private void Awake()
    {
        if (roomTrigger == null) roomTrigger = GetComponent<Collider2D>();
        if (waveManager == null) waveManager = GetComponent<WaveManager>();

        // 缓存房间范围（触发器禁用后仍可使用）
        cachedBounds = roomTrigger.bounds;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isActive) return;  // 已经激活过，忽略重复触发

        ActivateRoom(other.transform);
    }

    private void ActivateRoom(Transform player)
    {
        if(isCleared) return;

        if(isActive) return;

        isActive = true;

        // 关门
        foreach (var door in doors) door?.Close();

        // 激活所有现有敌人（如果有预先放置的敌人）
        foreach (var enemy in enemiesInRoom)
            enemy.runtime.target = player;

        // 启动波次
        waveManager?.StartWave(this);

        // 可选：禁用触发器，避免再次进入（也可保留，因为有 isActive 保护）
        // roomTrigger.enabled = false;
    }

    public Vector2 GetRandomValidPoint(float safeRadius = 0.5f)
    {
        // 使用缓存的房间范围
        for (int i = 0; i < 100; i++)
        {
            float x = Random.Range(cachedBounds.min.x, cachedBounds.max.x);
            float y = Random.Range(cachedBounds.min.y, cachedBounds.max.y);
            Vector2 point = new Vector2(x, y);

            // 检查点是否在触发器原始范围内（使用 cachedBounds 不够精确，但够用）
            // 如果房间形状不规则，建议保留 roomTrigger 的 OverlapPoint 但 roomTrigger 可能被禁用
            // 这里用缓存的边界框快速判断，对于矩形房间足够。
            // 如果需要精确判断，可以在 Awake 时克隆一个隐藏的碰撞器专门用于检测。

            Collider2D[] hits = Physics2D.OverlapCircleAll(point, safeRadius, obstacleMask);
            if (hits.Length == 0)
                return point;
        }
        return cachedBounds.center;
    }

    // 敌人注册/注销方法保持不变...
    public void RegisterEnemy(EnemyController enemy) => enemiesInRoom.Add(enemy);
    public void UnregisterEnemy(EnemyController enemy)
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
        foreach (var door in doors) door?.Open();
        isActive = false;  // 房间重置，允许后续再次进入（如果需要）
        Debug.Log("房间已清空，门已打开");
    }



}