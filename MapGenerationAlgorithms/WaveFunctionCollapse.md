# 波函数坍缩 (Wave Function Collapse) 地图生成算法

## 运行机理

波函数坍缩是一种基于约束满足的生成算法，灵感来自量子力学中的波函数坍缩概念。它通过逐步限制每个单元格的可能状态，最终生成一个满足所有局部约束的地图。

### 核心原理

1. **初始状态**：
   - 每个单元格都处于“叠加状态”，可能是任何允许的类型
   - 定义一组“瓷砖”（tile）作为基本构建块

2. **选择单元格**：
   - 选择具有最少可能状态的单元格
   - 这确保了算法优先处理最受约束的区域

3. **状态坍缩**：
   - 随机选择该单元格的一个可能状态
   - 该单元格的状态从叠加态坍缩为确定态

4. **传播约束**：
   - 检查与坍缩单元格相邻的所有单元格
   - 根据相邻单元格的状态，更新它们的可能状态
   - 移除与当前状态不兼容的可能性

5. **重复过程**：
   - 重复选择、坍缩和传播步骤
   - 直到所有单元格都被坍缩为确定状态

### 瓷砖集合

波函数坍缩算法依赖于预定义的瓷砖集合，每个瓷砖都有：
- 特定的形状和外观
- 与其他瓷砖的兼容性规则
- 出现概率（可选）

## 优势

1. **局部一致性**：
   - 生成的地图在局部区域具有高度一致性
   - 瓷砖之间的过渡自然流畅

2. **多样性**：
   - 每次生成的结果都不同
   - 可以生成复杂的模式和结构

3. **灵活性**：
   - 可以通过调整瓷砖集合来生成各种风格的地图
   - 支持不同维度的生成（2D、3D）

4. **可控性**：
   - 可以通过瓷砖设计和兼容性规则控制生成结果
   - 可以添加特殊瓷砖来确保特定结构的出现

## 适用场景

- **程序化纹理**：生成自然的纹理和图案
- **城市布局**：生成具有逻辑结构的城市布局
- **关卡设计**：创建具有特定主题的关卡
- **地形生成**：生成自然的地形和环境

## 实现示例

```csharp
public class WaveFunctionCollapse
{
    public class Tile
    {
        public string type;
        public Dictionary<Direction, List<string>> neighbors; // 每个方向允许的邻居类型
    }

    public class Cell
    {
        public int x, y;
        public List<string> possibleTiles;
        public string collapsedTile;
    }

    private Dictionary<string, Tile> tileSet;
    private Cell[,] grid;

    public void Initialize(int width, int height, Dictionary<string, Tile> tiles)
    {
        tileSet = tiles;
        grid = new Cell[width, height];

        // 初始化所有单元格为所有可能的瓷砖
        List<string> allTiles = new List<string>(tileSet.Keys);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell
                {
                    x = x,
                    y = y,
                    possibleTiles = new List<string>(allTiles),
                    collapsedTile = null
                };
            }
        }
    }

    public void Generate()
    {
        while (true)
        {
            // 选择具有最少可能状态的单元格
            Cell cell = GetCellWithMinimumEntropy();
            if (cell == null) break; // 所有单元格都已坍缩

            // 坍缩状态
            CollapseCell(cell);

            // 传播约束
            PropagateConstraints(cell);
        }
    }

    private Cell GetCellWithMinimumEntropy()
    {
        // 找到具有最少可能状态的未坍缩单元格
        Cell minCell = null;
        int minEntropy = int.MaxValue;

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Cell cell = grid[x, y];
                if (cell.collapsedTile == null && cell.possibleTiles.Count < minEntropy)
                {
                    minEntropy = cell.possibleTiles.Count;
                    minCell = cell;
                }
            }
        }

        return minCell;
    }

    private void CollapseCell(Cell cell)
    {
        // 随机选择一个可能的瓷砖
        int index = Random.Range(0, cell.possibleTiles.Count);
        cell.collapsedTile = cell.possibleTiles[index];
        cell.possibleTiles = new List<string> { cell.collapsedTile };
    }

    private void PropagateConstraints(Cell collapsedCell)
    {
        // 使用队列来传播约束
        Queue<Cell> queue = new Queue<Cell>();
        queue.Enqueue(collapsedCell);

        while (queue.Count > 0)
        {
            Cell cell = queue.Dequeue();

            // 检查所有邻居
            Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            foreach (Direction dir in directions)
            {
                int nx = cell.x + (int)dir.ToVector2().x;
                int ny = cell.y + (int)dir.ToVector2().y;

                // 检查边界
                if (nx < 0 || nx >= grid.GetLength(0) || ny < 0 || ny >= grid.GetLength(1))
                    continue;

                Cell neighbor = grid[nx, ny];
                if (neighbor.collapsedTile != null)
                    continue;

                // 计算邻居的可能瓷砖
                List<string> validTiles = new List<string>();
                foreach (string tileType in neighbor.possibleTiles)
                {
                    bool isValid = false;
                    foreach (string cellTile in cell.possibleTiles)
                    {
                        Tile cellTileData = tileSet[cellTile];
                        if (cellTileData.neighbors[dir.Opposite()].Contains(tileType))
                        {
                            isValid = true;
                            break;
                        }
                    }
                    if (isValid)
                        validTiles.Add(tileType);
                }

                // 如果可能的瓷砖发生变化，将邻居加入队列
                if (validTiles.Count != neighbor.possibleTiles.Count)
                {
                    neighbor.possibleTiles = validTiles;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
}
```

## 优化建议

1. **瓷砖设计**：设计具有丰富连接可能性的瓷砖集合
2. **性能优化**：使用更高效的数据结构来存储和更新可能状态
3. **种子控制**：添加种子系统，确保可以重现特定的生成结果
4. **约束权重**：为不同的约束添加权重，控制生成结果的倾向
5. **多尺度生成**：先在低分辨率生成，然后细化到高分辨率
6. **混合算法**：与其他生成算法结合使用，如先用BSP树创建房间结构，再用波函数坍缩填充细节