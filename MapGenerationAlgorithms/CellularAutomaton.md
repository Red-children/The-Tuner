# 细胞自动机 (Cellular Automaton) 地图生成算法

## 运行机理

细胞自动机是一种由大量简单单元（细胞）组成的系统，每个细胞根据其邻居的状态按照规则进行更新。在地图生成中，细胞自动机的工作原理如下：

1. **初始化网格**：
   - 创建一个二维网格，每个细胞代表一个地图单元格
   - 随机初始化细胞状态（墙或空地），通常设置较高的墙密度

2. **迭代更新**：
   - 对每个细胞应用一组规则，基于其邻居的状态
   - 常见规则：如果一个细胞周围的墙细胞数量在某个范围内，则该细胞变为墙

3. **平滑处理**：
   - 重复应用规则多次，直到地图达到稳定状态
   - 通常需要3-5次迭代

4. **连通性处理**：
   - 确保生成的地图是连通的
   - 移除孤立的区域

5. **房间识别**：
   - 识别生成的空地区域作为房间
   - 为房间添加入口和出口

## 优势

1. **自然外观**：
   - 生成的地图具有自然、有机的外观
   - 适合模拟洞穴、隧道等自然形成的环境

2. **多样性**：
   - 每次生成的地图都不同，具有高度随机性
   - 可以通过调整参数生成不同风格的地图

3. **灵活性**：
   - 可以通过修改规则和参数生成各种类型的地图
   - 适合生成复杂的地下结构

4. **性能**：
   - 算法实现简单，计算效率高
   - 适合生成大型地图

## 适用场景

- **洞穴探索游戏**：生成自然的洞穴系统
- **沙盒游戏**：创建随机生成的地下世界
- **生存游戏**：生成资源丰富的地下环境

## 实现示例

```csharp
public class CellularAutomaton
{
    public int[,] Generate(int width, int height, float fillDensity, int iterations)
    {
        // 初始化网格
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = Random.value < fillDensity ? 1 : 0; // 1 = 墙, 0 = 空地
            }
        }

        // 迭代更新
        for (int i = 0; i < iterations; i++)
        {
            map = UpdateMap(map);
        }

        return map;
    }

    private int[,] UpdateMap(int[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int[,] newMap = new int[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                int wallCount = CountNeighbors(map, x, y);
                if (wallCount >= 5) // 规则：如果周围有5个或更多墙，则变为墙
                    newMap[x, y] = 1;
                else if (wallCount <= 2) // 规则：如果周围墙少于3个，则变为空地
                    newMap[x, y] = 0;
                else
                    newMap[x, y] = map[x, y]; // 保持原状
            }
        }

        return newMap;
    }

    private int CountNeighbors(int[,] map, int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                if (map[x + i, y + j] == 1)
                    count++;
            }
        }
        return count;
    }
}
```

## 优化建议

1. **参数调优**：根据游戏需求调整填充密度和迭代次数
2. **多分辨率生成**：先在低分辨率生成，然后细化到高分辨率
3. **房间标记**：使用洪水填充算法识别和标记不同的房间
4. **走廊生成**：为识别出的房间添加走廊连接
5. **特殊区域**：在生成过程中标记特殊区域（如资源点、敌人营地等）