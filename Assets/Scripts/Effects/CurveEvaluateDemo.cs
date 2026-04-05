using UnityEngine;

/// <summary>
/// 曲线求值演示：展示AnimationCurve.Evaluate()方法的工作原理
/// 设计理念：通过实时调试信息帮助理解曲线求值过程
/// 机械工程类比：数控系统调试界面 - 实时显示运动参数
/// </summary>
public class CurveEvaluateDemo : MonoBehaviour
{
    [Header("测试曲线")]
    [SerializeField] private AnimationCurve testCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.2f);
    
    [Header("测试参数")]
    [SerializeField] private float testDuration = 2f;
    [SerializeField] private bool autoTest = true;
    [SerializeField] private float manualProgress = 0.5f;
    
    private float _timer = 0f;
    private bool _isTesting = false;
    
    private void Start()
    {
        if (autoTest)
        {
            StartAutoTest();
        }
        
        Debug.Log("曲线求值演示已启动");
        Debug.Log("按空格键开始/停止自动测试");
        Debug.Log("按1-9键测试特定进度(10%-90%)");
    }
    
    private void Update()
    {
        HandleInput();
        
        if (_isTesting && autoTest)
        {
            UpdateAutoTest();
        }
    }
    
    /// <summary>
    /// 处理用户输入
    /// </summary>
    private void HandleInput()
    {
        // 空格键：开始/停止自动测试
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleAutoTest();
        }
        
        // 数字键1-9：测试特定进度
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                TestSpecificProgress(i * 0.1f);
            }
        }
        
        // R键：重置测试
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTest();
        }
    }
    
    /// <summary>
    /// 开始自动测试
    /// </summary>
    private void StartAutoTest()
    {
        _isTesting = true;
        _timer = 0f;
        Debug.Log("自动测试开始");
    }
    
    /// <summary>
    /// 切换自动测试状态
    /// </summary>
    private void ToggleAutoTest()
    {
        _isTesting = !_isTesting;
        
        if (_isTesting)
        {
            _timer = 0f;
            Debug.Log("自动测试开始");
        }
        else
        {
            Debug.Log("自动测试停止");
        }
    }
    
    /// <summary>
    /// 更新自动测试
    /// </summary>
    private void UpdateAutoTest()
    {
        _timer += Time.deltaTime;
        
        if (_timer > testDuration)
        {
            _timer = 0f; // 循环测试
        }
        
        float progress = _timer / testDuration;
        EvaluateAndDisplay(progress);
    }
    
    /// <summary>
    /// 测试特定进度
    /// </summary>
    private void TestSpecificProgress(float progress)
    {
        EvaluateAndDisplay(progress);
        Debug.Log($"手动测试进度: {progress:P0}");
    }
    
    /// <summary>
    /// 重置测试
    /// </summary>
    private void ResetTest()
    {
        _timer = 0f;
        _isTesting = false;
        Debug.Log("测试已重置");
    }
    
    /// <summary>
    /// 执行曲线求值并显示结果
    /// </summary>
    private void EvaluateAndDisplay(float progress)
    {
        // 核心操作：曲线求值
        float result = testCurve.Evaluate(progress);
        
        // 显示详细信息
        DisplayEvaluationInfo(progress, result);
    }
    
    /// <summary>
    /// 显示求值信息
    /// </summary>
    private void DisplayEvaluationInfo(float progress, float result)
    {
        // 控制台输出
        if (Time.frameCount % 30 == 0) // 每30帧输出一次，避免刷屏
        {
            Debug.Log($"进度: {progress:P1} → 求值结果: {result:F3}");
        }
        
        // 可视化进度条（在场景中显示）
        DisplayVisualProgress(progress, result);
    }
    
    /// <summary>
    /// 在场景中显示可视化进度
    /// </summary>
    private void DisplayVisualProgress(float progress, float result)
    {
        // 这里可以添加GUI显示或3D可视化
        // 简化实现：使用Debug绘制
        
        Vector3 startPos = transform.position + Vector3.left * 2f;
        Vector3 endPos = transform.position + Vector3.right * 2f;
        Vector3 currentPos = Vector3.Lerp(startPos, endPos, progress);
        
        // 绘制进度线
        Debug.DrawLine(startPos, endPos, Color.gray, 0.1f);
        
        // 绘制当前位置
        Debug.DrawLine(currentPos, currentPos + Vector3.up * result, Color.red, 0.1f);
        
        // 绘制结果高度
        Debug.DrawLine(currentPos, currentPos + Vector3.up * result, Color.green, 0.1f);
    }
    
    /// <summary>
    /// 在GUI中显示调试信息
    /// </summary>
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        
        GUILayout.Label("=== 曲线求值演示 ===");
        GUILayout.Label($"当前曲线: {GetCurveDescription()}");
        GUILayout.Label($"测试状态: {(_isTesting ? "运行中" : "已停止")}");
        GUILayout.Label($"当前进度: {(_timer / testDuration):P1}");
        
        float currentProgress = _timer / testDuration;
        float currentResult = testCurve.Evaluate(currentProgress);
        
        GUILayout.Label($"求值结果: {currentResult:F3}");
        GUILayout.Label("");
        
        GUILayout.Label("操作说明:");
        GUILayout.Label("空格键 - 开始/停止自动测试");
        GUILayout.Label("1-9键 - 测试10%-90%进度");
        GUILayout.Label("R键 - 重置测试");
        
        // 显示曲线预览
        GUILayout.Label("曲线预览:");
        DisplayCurvePreview();
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 获取曲线描述
    /// </summary>
    private string GetCurveDescription()
    {
        if (testCurve.length >= 2)
        {
            Keyframe start = testCurve[0];
            Keyframe end = testCurve[testCurve.length - 1];
            return $"({start.time:F1}, {start.value:F1}) → ({end.time:F1}, {end.value:F1})";
        }
        return "自定义曲线";
    }
    
    /// <summary>
    /// 显示简化的曲线预览
    /// </summary>
    private void DisplayCurvePreview()
    {
        // 简化实现：显示几个关键点的值
        GUILayout.BeginHorizontal();
        
        for (int i = 0; i <= 10; i++)
        {
            float progress = i * 0.1f;
            float value = testCurve.Evaluate(progress);
            GUILayout.Label($"{progress:P0}:{value:F2}", GUILayout.Width(40));
        }
        
        GUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 测试不同的曲线类型
    /// </summary>
    public void TestDifferentCurves()
    {
        // 线性曲线
        AnimationCurve linearCurve = AnimationCurve.Linear(0, 1, 1, 0.2f);
        TestCurve("线性曲线", linearCurve);
        
        // 缓入曲线
        AnimationCurve easeInCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.2f);
        TestCurve("缓入缓出曲线", easeInCurve);
        
        // 自定义曲线
        AnimationCurve customCurve = new AnimationCurve(
            new Keyframe(0, 1),
            new Keyframe(0.3f, 0.8f),
            new Keyframe(0.7f, 0.3f),
            new Keyframe(1, 0.2f)
        );
        TestCurve("自定义曲线", customCurve);
    }
    
    /// <summary>
    /// 测试特定曲线
    /// </summary>
    private void TestCurve(string curveName, AnimationCurve curve)
    {
        Debug.Log($"=== 测试{curveName} ===");
        
        for (int i = 0; i <= 10; i++)
        {
            float progress = i * 0.1f;
            float result = curve.Evaluate(progress);
            Debug.Log($"进度 {progress:P0} → 结果 {result:F3}");
        }
    }
}