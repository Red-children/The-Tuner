# 基于图的地图生成算法

## 运行机理

基于图的地图生成算法使用图数据结构来表示房间之间的连接关系。这种方法将地图生成分为两个主要阶段：拓扑生成和几何生成。

### 拓扑生成阶段

1. **初始节点创建**：
   - 创建一个起始节点（通常是起点房间）
   - 从起始节点开始，逐步添加新节点

2. **节点扩展**：
   - 随机选择一个现有节点
   - 随机选择一个未使用的方向
   - 在该方向创建一个新节点
   - 标记节点之间的连接

3. **终止条件**：
   - 当达到预设的节点数量时停止
   - 或当没有更多可扩展的节点时停止

### 几何生成阶段

1. **房间放置**：
   - 为每个节点分配一个物理位置
   - 确保房间之间有足够的空间

2. **走廊生成**：
   - 为图中的每条边创建连接走廊
   - 确保走廊连接对应的房间入口

3. **后处理**：
   - 调整房间大小和形状
   - 确保所有房间和走廊的连通性

## 优势

1. **高度可控**：
   - 可以精确控制房间数量和连接方式
   - 可以根据需要创建特定的地图结构

2. **灵活性**：
   - 支持各种房间类型和大小
   - 可以生成复杂的地图结构

3. **可扩展性**：
   - 易于添加新的房间类型和生成规则
   - 可以与其他生成算法结合使用

4. **清晰的结构**：
   - 生成的地图具有明确的房间和走廊结构
   - 便于实现游戏逻辑和AI导航

## 适用场景

- **程序化地牢**：生成具有明确房间结构的地牢
- **关卡设计**：创建具有特定布局要求的关卡
- **开放世界**：生成由多个区域组成的开放世界

## 实现示例

```csharp
public class GraphBasedGenerator
{
    public class Node
    {
        public Vector2 position;
        public RoomType type;
        public List<Edge> edges = new List<Edge>();
    }

    public class Edge
    {
        public Node from;
        public Node to;
        public Direction direction;
    }

    public List<Node> Generate(int roomCount, RoomType startType)
    {
        List<Node> nodes = new List<Node>();
        
        // 创建起始节点
        Node startNode = new Node { type = startType, position = Vector2.zero };
        nodes.Add(startNode);

        // 扩展节点
        while (nodes.Count < roomCount)
        {
            // 随机选择一个节点
            Node current = nodes[Random.Range(0, nodes.Count)];
            
            // 尝试在随机方向创建新节点
            Direction direction = (Direction)Random.Range(0, 4);
            Vector2 newPosition = current.position + direction.ToVector2();
            
            // 检查位置是否已被占用
            bool positionTaken = nodes.Any(n => n.position == newPosition);
            if (positionTaken) continue;
            
            // 创建新节点
            Node newNode = new Node
            {
                type = ChooseRoomType(nodes.Count),
                position = newPosition
            };
            
            // 添加连接
            Edge edge = new Edge
            {
                from = current,
                to = newNode,
                direction = direction
            };
            current.edges.Add(edge);
            
            nodes.Add(newNode);
        }
        
        return nodes;
    }

    private RoomType ChooseRoomType(int depth)
    {
        // 根据深度选择房间类型
        float rand = Random.value;
        if (rand < 0.7f) return RoomType.Normal;
        else if (rand < 0.9f) return RoomType.Elite;
        else return RoomType.Treasure;
    }
}
```

## 优化建议

1. **房间类型分布**：根据深度和位置调整房间类型的分布
2. **路径长度控制**：确保从起点到终点的路径长度合理
3. **空间优化**：使用更高效的空间分配算法，避免房间重叠
4. **连接多样性**：添加多种类型的连接方式，如直接连接、走廊连接等
5. **地图主题**：根据节点位置或类型添加主题元素，增强地图的一致性