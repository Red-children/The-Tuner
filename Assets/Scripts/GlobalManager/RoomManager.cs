using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 房间枚举
public enum RoomType
{
    Start,          // 起点
    Normal,         // 普通战斗房
    Elite,          // 精英怪房
    Treasure,       // 宝箱房
    Shop,           // 商店
    Boss,           // Boss房
    End             // 终点（通关）
}
#endregion

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [System.Serializable]
    public class RoomPrefabEntry
    {
        public RoomType type;
        public List<GameObject> prefabs;
    }

    public List<RoomPrefabEntry> roomPrefabs;      // 在 Inspector 中配置
    public int targetRoomCount = 10;               // 目标房间总数（含起点）
    public Transform dungeonRoot;                  // 生成房间的父物体
    public float roomHeight = 20f;                 // 房间高度（用于重叠检测和简单偏移，可根据实际调整）
    public float roomWidth = 20f;                  // 房间宽度

    private List<GameObject> spawnedRooms = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        if (dungeonRoot == null)
            dungeonRoot = transform;
    }

    private void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        ClearDungeon();
        StartCoroutine(GenerateCoroutine());
    }

    private void ClearDungeon()
    {
        foreach (var room in spawnedRooms)
            Destroy(room);
        spawnedRooms.Clear();
    }
   
    private IEnumerator GenerateCoroutine()
    {
        // 1. 生成起点房间
        GameObject startRoom = GetRandomPrefab(RoomType.Start);
        if (startRoom == null)
        {
            Debug.LogError("没有起点房间预制体！");
            yield break;
        }
        GameObject startInstance = Instantiate(startRoom, dungeonRoot.position, Quaternion.identity, dungeonRoot);
        spawnedRooms.Add(startInstance);

        // 获取起点房间的门口列表（世界坐标下的 Transform）
        List<Transform> availableDoors = GetDoorTransforms(startInstance);
        Debug.Log($"起点房间可用门口数: {availableDoors.Count}");
        yield return null;

        // 2. 循环生成房间
        while (spawnedRooms.Count < targetRoomCount && availableDoors.Count > 0)
        {
            // 随机选一个门口
            Transform currentDoor = availableDoors[Random.Range(0, availableDoors.Count)];
            availableDoors.Remove(currentDoor);
            Vector2 currentDir = currentDoor.up;

            // 根据方向计算偏移
            Vector3 offset = Vector3.zero;
            if (currentDir == Vector2.up) offset = Vector3.up * roomHeight;
            else if (currentDir == Vector2.down) offset = Vector3.down * roomHeight;
            else if (currentDir == Vector2.left) offset = Vector3.left * roomWidth;
            else if (currentDir == Vector2.right) offset = Vector3.right * roomWidth;

            // 新房间中心位置
            Vector3 newPos = currentDoor.position + offset;

            // 随机选择房间类型
            GameObject prefab = GetRandomPrefab(ChooseRoomType());
            if (prefab == null) continue;

            // 计算新房间的旋转
            Quaternion newRot = Quaternion.identity;
            if (currentDir == Vector2.up) newRot = Quaternion.Euler(0, 0, 180); // 上门口朝上，新房间旋转180度使其下门口朝下
            else if (currentDir == Vector2.down) newRot = Quaternion.identity;
            else if (currentDir == Vector2.left) newRot = Quaternion.Euler(0, 0, 90);
            else if (currentDir == Vector2.right) newRot = Quaternion.Euler(0, 0, -90);

            // 实例化新房间
            GameObject newRoom = Instantiate(prefab, newPos, newRot, dungeonRoot);
            spawnedRooms.Add(newRoom);

            // 获取新房间的门口列表（世界坐标下的 Transform）
            List<Transform> newRoomDoors = GetDoorTransforms(newRoom);

            // 找到新房间中方向与当前门口相反的那个门（即被占用的门）
            Transform matchedRealDoor = FindMatchingDoor(newRoomDoors, currentDir);
            if (matchedRealDoor != null)
                newRoomDoors.Remove(matchedRealDoor);

            // 剩余门口加入可用列表
            availableDoors.AddRange(newRoomDoors);

            // ... 其他初始化 ...
            yield return null;
        }
    }

    // 获取 GameObject 上所有带有 "Doorway" 标签的子物体的 Transform
    private List<Transform> GetDoorTransforms(GameObject obj)
    {
        List<Transform> doors = new List<Transform>();
        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag("Doorway"))
                doors.Add(child);
        }
        return doors;
    }

    // 在门口列表中查找方向与目标方向相反的门口（允许误差）
    private Transform FindMatchingDoor(List<Transform> doors, Vector2 targetDir)
    {
        foreach (var door in doors)
        {
            if (Vector2.Dot(door.up, targetDir) < -0.9f)  // 方向相反
                return door;
        }
        return null;
    }

    private GameObject GetRandomPrefab(RoomType type)
    {
        var entry = roomPrefabs.Find(e => e.type == type);
        if (entry == null || entry.prefabs.Count == 0) return null;
        return entry.prefabs[Random.Range(0, entry.prefabs.Count)];
    }

    private RoomType ChooseRoomType()
    {
        // 简单概率：80%普通，20%精英
        float rand = Random.value;
        if (rand < 0.8f)
            return RoomType.Normal;
        else
            return RoomType.Elite;
    }

    private void EnsureBossRoom()
    {
        // 示例：可以将最后一个房间改为 Boss 房（实际需替换预制体或修改配置）
        // 这里留空，按需实现
    }
}