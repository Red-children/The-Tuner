using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("房间组件")]
    public Collider2D roomTrigger;          // 房间触发器（用于检测玩家进入）
    public WaveManager waveManager;
    public Door[] doors;
    public LayerMask obstacleMask;

    private Bounds cachedBounds;             // 缓存房间范围（用于敌人生成）
    public bool isActive = false;           // 房间是否已被激活
   
    public List<EnemyBase> enemiesInRoom = new List<EnemyBase>();

    private int totalEnemies = 0;    // 总可杀敌数
    private int killedCount = 0;     // 已击杀数
    public bool isCleared = false;

    public void SetTotalEnemies(int total)
    {
        totalEnemies = total;
        killedCount = 0;
    }

    public void UnregisterEnemy(EnemyBase enemy)
    {
        enemiesInRoom.Remove(enemy);
        killedCount++;
        
        // 简化注销逻辑：只更新数据，不进行完成检测
        // 完成检测由独立的Update方法处理
        Debug.Log($"房间 {name} 敌人注销: {enemy.name}, 剩余敌人: {enemiesInRoom.Count}");
    }

    private void Awake()
    {
        if (roomTrigger == null) roomTrigger = GetComponent<Collider2D>();
        if (waveManager == null) waveManager = GetComponent<WaveManager>();

        // 缓存房间范围，生成敌人时可以使用
        cachedBounds = roomTrigger.bounds;
    }

    private void Start() // 或 Awake
    {
        // 初始状态：门关闭，房间未激活
        foreach (var door in doors)
            door?.Close();
        isActive = false;
        isCleared = false;
    }

    private void Update()
    {
        // 独立的房间完成检测逻辑
        // 避免在敌人注销时检测，确保检测时机正确
        CheckRoomCompletion();
    }

    /// <summary>
    /// 检查房间是否完成（所有敌人都被消灭且所有波次已完成）
    /// </summary>
    private void CheckRoomCompletion()
    {
        if (isCleared) return;
        if (!isActive) return; // 新增：只有房间激活后才检测

        if (enemiesInRoom.Count == 0 && waveManager != null && !waveManager.isWaveActive)
        {
            isCleared = true;
            OnRoomCleared();
            Debug.Log($"房间 {name} 清理完成，打开门");
        }
    }

    private void OnEnable()
    {
         // 订阅敌人死亡事件
         EventBus.Instance.Subscribe<EnemyDiedStruct>(OnEnemyDied);
    }

    private void OnDisable()
    {
        // 取消订阅敌人死亡事件
        EventBus.Instance.Unsubscribe<EnemyDiedStruct>(OnEnemyDied);
    }

    /// <summary>
    /// 处理敌人死亡事件
    /// </summary>
    private void OnEnemyDied(EnemyDiedStruct evt)
    {
        // 检查死亡的敌人是否在这个房间中
        if (evt.enemy != null && enemiesInRoom.Contains(evt.enemy))
        {
            // 从房间中注销这个敌人
            UnregisterEnemy(evt.enemy);
            Debug.Log($"房间 {name} 处理了敌人死亡: {evt.enemy.name}");
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isActive) return;  // 已经激活过，避免重复激活

        ActivateRoom(other.transform);
        Debug.Log($"玩家进入房间 {name}，激活房间");
    }

    private void ActivateRoom(Transform player)
    {
        if(isCleared) return;

        if(isActive) return;

        isActive = true;

        // 关门
        foreach (var door in doors) door?.Close();

        // 激活房间内所有的敌人（包括预置的敌人）
        foreach (var enemy in enemiesInRoom)
        {
          
            if (enemy != null )
            {
                enemy.target = player;
            }
        }

        // 生成敌人
        waveManager?.StartWave(this);

        // 可选：禁用触发器，防止玩家再次进入（也可以保留为 isActive 标志）
        // roomTrigger.enabled = false;
    }

    #region 生成敌人在有效点
    //生成敌人在房间内的有效点
    public Vector2 GetRandomValidPoint(float safeRadius = 0.5f)
    {
        // 使用缓存的房间范围
        for (int i = 0; i < 100; i++)
        {
            float x = Random.Range(cachedBounds.min.x, cachedBounds.max.x);
            float y = Random.Range(cachedBounds.min.y, cachedBounds.max.y);
            Vector2 point = new Vector2(x, y);

            // 检查是否在障碍物内
            Collider2D[] hits = Physics2D.OverlapCircleAll(point, safeRadius, obstacleMask);
            if (hits.Length == 0)
                return point;
        }
        return cachedBounds.center;
    }
    #endregion

    // 敌人注册
    public void RegisterEnemy(EnemyBase enemy) => enemiesInRoom.Add(enemy);



    #region 房间清理逻辑
    private void OnRoomCleared()
    {
        foreach (var door in doors) door?.Open();
        isActive = false;  // 重置激活状态，允许玩家再次进入（如果需要）
        Debug.Log("房间清理完成，打开门");
    }
    #endregion
}