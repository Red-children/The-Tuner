using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Start,      // 起点房间
    Normal,     // 普通战斗房
    Elite,      // 精英房
    Treasure,   // 宝箱房
    Shop,       // 商店
    Boss,       // Boss房
    End         // 终点（通关）
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public static class DirectionExtensions
{
    public static Vector2Int ToVector2Int(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return new Vector2Int(0, 1);
            case Direction.Down: return new Vector2Int(0, -1);
            case Direction.Left: return new Vector2Int(-1, 0);
            case Direction.Right: return new Vector2Int(1, 0);
            default: return Vector2Int.zero;
        }
    }

    public static Direction Opposite(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Direction.Down;
            case Direction.Down: return Direction.Up;
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            default: return dir;
        }
    }
}

[System.Serializable]
public class RoomPrefabEntry
{
    public RoomType type;
    public List<GameObject> prefabs;
}

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("房间预制体库")]
    public List<RoomPrefabEntry> roomPrefabs;

    [Header("房间尺寸（所有房间必须相同）")]
    public float roomWidth = 20f;
    public float roomHeight = 20f;

    [Header("生成参数")]
    public int targetRoomCount = 10;          // 总房间数（含起点）
    public Transform dungeonRoot;              // 生成房间的父物体

    // 内部数据结构
    private class RoomNode
    {
        public GameObject prefab;
        public Vector2Int gridPos;
        public GameObject instance;
        public bool[] doorsUsed = new bool[4]; // 0:上,1:下,2:左,3:右
        public RoomType type;

        public RoomNode(GameObject prefab, Vector2Int pos, RoomType type)
        {
            this.prefab = prefab;
            this.gridPos = pos;
            this.type = type;
        }
    }

    private List<RoomNode> nodes = new List<RoomNode>();
    private Dictionary<Vector2Int, RoomNode> gridMap = new Dictionary<Vector2Int, RoomNode>();

    private void Awake()
    {
        Instance = this;
        if (dungeonRoot == null) dungeonRoot = transform;
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
        foreach (var node in nodes)
        {
            if (node.instance != null) Destroy(node.instance);
        }
        nodes.Clear();
        gridMap.Clear();
    }

    // 随机获取某个类型的房间预制体
    private GameObject GetRandomPrefab(RoomType type)
    {
        var entry = roomPrefabs.Find(e => e.type == type);
        if (entry == null || entry.prefabs.Count == 0) return null;
        return entry.prefabs[Random.Range(0, entry.prefabs.Count)];
    }

    // 根据当前节点和深度选择房间类型（可扩展难度曲线）
    private RoomType ChooseRoomType(int depth = 0)
    {
        // 简单概率：80%普通，20%精英
        float rand = Random.value;
        if (rand < 0.8f) return RoomType.Normal;
        else return RoomType.Elite;
    }

    // 生成房间拓扑图
    private List<RoomNode> GenerateGraph()
    {
        List<RoomNode> graph = new List<RoomNode>();

        // 起点房间
        GameObject startPrefab = GetRandomPrefab(RoomType.Start);
        if (startPrefab == null)
        {
            Debug.LogError("没有起点房间预制体！");
            return graph;
        }
        RoomNode startNode = new RoomNode(startPrefab, Vector2Int.zero, RoomType.Start);
        graph.Add(startNode);
        gridMap[Vector2Int.zero] = startNode;

        // 使用随机生长算法添加房间
        while (graph.Count < targetRoomCount)
        {
            // 随机选择一个已存在的节点
            RoomNode current = graph[Random.Range(0, graph.Count)];
            // 收集当前节点所有未使用的门口方向
            List<Direction> freeDirs = new List<Direction>();
            for (int i = 0; i < 4; i++)
            {
                if (!current.doorsUsed[i]) freeDirs.Add((Direction)i);
            }
            if (freeDirs.Count == 0) continue; // 当前节点无空闲门口，换一个

            Direction dir = freeDirs[Random.Range(0, freeDirs.Count)];
            Vector2Int newPos = current.gridPos + dir.ToVector2Int();

            // 检查新位置是否已被占用
            if (gridMap.ContainsKey(newPos)) continue;

            // 选择新房间类型
            RoomType type = ChooseRoomType();
            GameObject prefab = GetRandomPrefab(type);
            if (prefab == null) continue;

            // 创建新节点
            RoomNode newNode = new RoomNode(prefab, newPos, type);
            // 标记新节点的对应门口为已使用（方向与当前方向相反）
            Direction opposite = dir.Opposite();
            newNode.doorsUsed[(int)opposite] = true;
            // 标记当前节点的门口为已使用
            current.doorsUsed[(int)dir] = true;

            graph.Add(newNode);
            gridMap[newPos] = newNode;
        }

        return graph;
    }

    // 实例化房间
    private IEnumerator GenerateCoroutine()
    {
        // 1. 生成图结构
        nodes = GenerateGraph();
        if (nodes.Count == 0) yield break;

        // 2. 计算世界坐标并实例化房间
        Vector2 origin = dungeonRoot.position; // 起点房间中心
        float offsetX = roomWidth;
        float offsetY = roomHeight;

        foreach (var node in nodes)
        {
            Vector3 worldPos = new Vector3(
                origin.x + node.gridPos.x * offsetX,
                origin.y + node.gridPos.y * offsetY,
                0
            );

            GameObject roomObj = Instantiate(node.prefab, worldPos, Quaternion.identity, dungeonRoot);
            node.instance = roomObj;

            // 初始化房间（配置波次等）
            Room roomComp = roomObj.GetComponent<Room>();
            if (roomComp != null) roomComp.Init(node.type);

            yield return null; // 分帧实例化，避免卡顿
        }

        // 3. 后处理：确保Boss房在终点等（可选）
        EnsureBossRoom();

        Debug.Log($"地图生成完成，共生成 {nodes.Count} 个房间");
    }

    private void EnsureBossRoom()
    {
        // 简单示例：如果房间数达标，将最后一个房间改为Boss房（这里只是示意，实际可能需要替换预制体）
        // 你可以根据节点位置或路径长度来替换房间类型
        if (nodes.Count >= targetRoomCount && nodes[nodes.Count - 1].type != RoomType.Boss)
        {
            // 替换最后一个房间的预制体为Boss房（但需要保持连接，可能会破坏门口）
            // 这里建议在生成图时就决定Boss房，或者在生成后调整类型并替换预制体。
            // 暂时留空，按需实现。
        }
    }
}