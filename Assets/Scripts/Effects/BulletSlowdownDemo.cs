using UnityEngine;
using System.Collections;

/// <summary>
/// 子弹减速演示场景：展示三种不同减速方案的实际应用
/// 设计理念：通过实际演示帮助理解不同方案的优缺点和适用场景
/// 机械工程类比：汽车测试场 - 不同车型在不同路况下的性能对比
/// </summary>
public class BulletSlowdownDemo : MonoBehaviour
{
    [Header("演示模式设置")]
    [SerializeField] private DemoMode demoMode = DemoMode.Comparison;
    [SerializeField] private bool enableAutoDemo = true;
    [SerializeField] private float demoInterval = 3f;
    
    [Header("子弹预设")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform spawnPoint;
    
    [Header("目标设置")]
    [SerializeField] private Transform[] targetPoints;
    [SerializeField] private MaterialType[] targetMaterials;
    
    [Header("减速方案配置")]
    [SerializeField] private SlowdownConfig physicsConfig;
    [SerializeField] private SlowdownConfig curveConfig;
    [SerializeField] private SlowdownConfig componentConfig;
    
    [Header("演示控制")]
    [SerializeField] private KeyCode spawnKey = KeyCode.Space;
    [SerializeField] private KeyCode switchModeKey = KeyCode.Tab;
    [SerializeField] private KeyCode testPhysicsKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode testCurveKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode testComponentKey = KeyCode.Alpha3;
    
    private int _currentDemoIndex = 0;
    private Coroutine _autoDemoCoroutine;
    private GameObject _currentBullet;
    
    private void Start()
    {
        if (enableAutoDemo)
        {
            StartAutoDemo();
        }
        
        Debug.Log("子弹减速演示场景已启动");
        Debug.Log("按键说明：");
        Debug.Log("空格键 - 生成测试子弹");
        Debug.Log("Tab键 - 切换演示模式");
        Debug.Log("1键 - 测试物理模拟方案");
        Debug.Log("2键 - 测试曲线控制方案");
        Debug.Log("3键 - 测试组件化方案");
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    /// <summary>
    /// 处理用户输入
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnTestBullet();
        }
        
        if (Input.GetKeyDown(switchModeKey))
        {
            SwitchDemoMode();
        }
        
        if (Input.GetKeyDown(testPhysicsKey))
        {
            TestPhysicsSlowdown();
        }
        
        if (Input.GetKeyDown(testCurveKey))
        {
            TestCurveSlowdown();
        }
        
        if (Input.GetKeyDown(testComponentKey))
        {
            TestComponentSlowdown();
        }
    }
    
    /// <summary>
    /// 开始自动演示
    /// </summary>
    private void StartAutoDemo()
    {
        if (_autoDemoCoroutine != null)
        {
            StopCoroutine(_autoDemoCoroutine);
        }
        
        _autoDemoCoroutine = StartCoroutine(AutoDemoRoutine());
    }
    
    /// <summary>
    /// 自动演示协程
    /// </summary>
    private IEnumerator AutoDemoRoutine()
    {
        while (enableAutoDemo)
        {
            yield return new WaitForSeconds(demoInterval);
            
            // 生成测试子弹
            SpawnTestBullet();
            
            // 根据当前演示模式执行测试
            ExecuteDemoTest();
            
            // 切换到下一个演示
            _currentDemoIndex = (_currentDemoIndex + 1) % 3;
        }
    }
    
    /// <summary>
    /// 执行演示测试
    /// </summary>
    private void ExecuteDemoTest()
    {
        switch (_currentDemoIndex)
        {
            case 0:
                TestPhysicsSlowdown();
                Debug.Log("自动演示: 物理模拟方案");
                break;
            case 1:
                TestCurveSlowdown();
                Debug.Log("自动演示: 曲线控制方案");
                break;
            case 2:
                TestComponentSlowdown();
                Debug.Log("自动演示: 组件化方案");
                break;
        }
    }
    
    /// <summary>
    /// 生成测试子弹
    /// </summary>
    private void SpawnTestBullet()
    {
        if (bulletPrefab == null || spawnPoint == null)
        {
            Debug.LogError("子弹预设或生成点未设置");
            return;
        }
        
        // 销毁之前的子弹
        if (_currentBullet != null)
        {
            Destroy(_currentBullet);
        }
        
        // 生成新子弹
        _currentBullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        
        Debug.Log("测试子弹已生成");
    }
    
    /// <summary>
    /// 测试物理模拟方案
    /// </summary>
    private void TestPhysicsSlowdown()
    {
        if (_currentBullet == null)
        {
            Debug.LogWarning("没有可用的测试子弹");
            return;
        }
        
        // 添加物理模拟组件
        var physicsSlowdown = _currentBullet.GetComponent<BulletPhysicsSlowdown>();
        if (physicsSlowdown == null)
        {
            physicsSlowdown = _currentBullet.AddComponent<BulletPhysicsSlowdown>();
        }
        
        // 配置物理参数
        ConfigurePhysicsSlowdown(physicsSlowdown);
        
        // 模拟碰撞测试
        StartCoroutine(SimulatePhysicsCollision(physicsSlowdown));
        
        Debug.Log("物理模拟方案测试开始");
    }
    
    /// <summary>
    /// 测试曲线控制方案
    /// </summary>
    private void TestCurveSlowdown()
    {
        if (_currentBullet == null)
        {
            Debug.LogWarning("没有可用的测试子弹");
            return;
        }
        
        // 添加曲线控制组件
        var curveSlowdown = _currentBullet.GetComponent<BulletCurveSlowdown>();
        if (curveSlowdown == null)
        {
            curveSlowdown = _currentBullet.AddComponent<BulletCurveSlowdown>();
        }
        
        // 配置曲线参数
        ConfigureCurveSlowdown(curveSlowdown);
        
        // 触发曲线减速
        curveSlowdown.TriggerCurveSlowdown(GetRandomRank());
        
        Debug.Log("曲线控制方案测试开始");
    }
    
    /// <summary>
    /// 测试组件化方案
    /// </summary>
    private void TestComponentSlowdown()
    {
        if (_currentBullet == null)
        {
            Debug.LogWarning("没有可用的测试子弹");
            return;
        }
        
        // 添加组件化系统
        var componentSystem = _currentBullet.GetComponent<BulletSlowdownSystem>();
        if (componentSystem == null)
        {
            componentSystem = _currentBullet.AddComponent<BulletSlowdownSystem>();
        }
        
        // 配置组件系统
        ConfigureComponentSystem(componentSystem);
        
        // 触发组件减速
        componentSystem.TriggerComponentSlowdown(GetRandomRank());
        
        Debug.Log("组件化方案测试开始");
    }
    
    /// <summary>
    /// 配置物理模拟方案
    /// </summary>
    private void ConfigurePhysicsSlowdown(BulletPhysicsSlowdown physics)
    {
        if (physicsConfig.isConfigured)
        {
            physics.kineticEnergyLoss = physicsConfig.kineticEnergyLoss;
            physics.materialHardness = physicsConfig.materialHardness;
            physics.slowdownDuration = physicsConfig.duration;
        }
    }
    
    /// <summary>
    /// 配置曲线控制方案
    /// </summary>
    private void ConfigureCurveSlowdown(BulletCurveSlowdown curve)
    {
        if (curveConfig.isConfigured)
        {
            curve.slowdownCurve = curveConfig.slowdownCurve;
            curve.recoveryCurve = curveConfig.recoveryCurve;
        }
    }
    
    /// <summary>
    /// 配置组件化系统
    /// </summary>
    private void ConfigureComponentSystem(BulletSlowdownSystem system)
    {
        if (componentConfig.isConfigured)
        {
            // 这里可以添加组件配置逻辑
        }
    }
    
    /// <summary>
    /// 模拟物理碰撞
    /// </summary>
    private IEnumerator SimulatePhysicsCollision(BulletPhysicsSlowdown physics)
    {
        // 等待子弹移动一小段距离
        yield return new WaitForSeconds(0.5f);
        
        // 模拟碰撞
        var collision = CreateSimulatedCollision();
        MaterialType material = GetRandomMaterial();
        
        // 触发物理减速
        physics.TriggerPhysicsSlowdown(collision, material);
        
        Debug.Log($"模拟物理碰撞: 材质={material}, 角度={collision.contacts[0].normal}");
    }
    
    /// <summary>
    /// 创建模拟碰撞
    /// </summary>
    private Collision2D CreateSimulatedCollision()
    {
        // 简化实现：在实际项目中需要更复杂的碰撞模拟
        // 这里返回一个空的碰撞对象用于演示
        return new Collision2D();
    }
    
    /// <summary>
    /// 获取随机节奏判定
    /// </summary>
    private RhythmRank GetRandomRank()
    {
        RhythmRank[] ranks = { RhythmRank.Perfect, RhythmRank.Great, RhythmRank.Good, RhythmRank.Miss };
        return ranks[Random.Range(0, ranks.Length)];
    }
    
    /// <summary>
    /// 获取随机材质
    /// </summary>
    private MaterialType GetRandomMaterial()
    {
        MaterialType[] materials = { MaterialType.Soft, MaterialType.Normal, MaterialType.Hard, MaterialType.Metal };
        return materials[Random.Range(0, materials.Length)];
    }
    
    /// <summary>
    /// 切换演示模式
    /// </summary>
    private void SwitchDemoMode()
    {
        demoMode = (DemoMode)(((int)demoMode + 1) % 3);
        
        Debug.Log($"演示模式切换为: {demoMode}");
        
        // 根据新模式重新开始演示
        if (enableAutoDemo)
        {
            StartAutoDemo();
        }
    }
    
    /// <summary>
    /// 显示方案对比信息
    /// </summary>
    private void ShowComparisonInfo()
    {
        Debug.Log("=== 子弹减速方案对比 ===");
        Debug.Log("1. 物理模拟方案:");
        Debug.Log("   - 优点: 真实感强，符合物理直觉");
        Debug.Log("   - 缺点: 计算复杂度较高");
        Debug.Log("   - 适用: 追求物理真实性的游戏");
        
        Debug.Log("2. 曲线控制方案:");
        Debug.Log("   - 优点: 精确可控，支持复杂效果");
        Debug.Log("   - 缺点: 需要精细调校");
        Debug.Log("   - 适用: 需要精细调校的节奏游戏");
        
        Debug.Log("3. 组件化方案:");
        Debug.Log("   - 优点: 模块化，易于扩展和维护");
        Debug.Log("   - 缺点: 架构复杂度最高");
        Debug.Log("   - 适用: 大型项目，需要灵活配置");
    }
    
    /// <summary>
    /// 在场景中显示调试信息
    /// </summary>
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== 子弹减速演示场景 ===");
        GUILayout.Label($"当前模式: {demoMode}");
        GUILayout.Label($"演示索引: {_currentDemoIndex}");
        GUILayout.Label("");
        GUILayout.Label("按键说明:");
        GUILayout.Label("空格键 - 生成测试子弹");
        GUILayout.Label("Tab键 - 切换演示模式");
        GUILayout.Label("1键 - 测试物理模拟方案");
        GUILayout.Label("2键 - 测试曲线控制方案");
        GUILayout.Label("3键 - 测试组件化方案");
        GUILayout.EndArea();
    }
}

// 演示模式枚举
public enum DemoMode
{
    Comparison,    // 对比模式：依次展示三种方案
    SingleTest,    // 单测模式：专注于一种方案
    Interactive    // 交互模式：用户手动控制
}

// 减速配置结构
[System.Serializable]
public struct SlowdownConfig
{
    public bool isConfigured;
    public float kineticEnergyLoss;
    public float materialHardness;
    public float duration;
    public AnimationCurve slowdownCurve;
    public AnimationCurve recoveryCurve;
}