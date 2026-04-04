using UnityEngine;

/// <summary>
/// NPC主控脚本：简洁实用的模块化管理
/// 设计理念：基于项目现有模式，避免过度设计
/// </summary>
public class NPCController : MonoBehaviour
{
    [Header("NPC基本信息")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private bool startEnabled = true;
    
    [Header("模块引用")]
    [SerializeField] private NPCCommunication communication;
    
    // 简单状态标记
    private bool _isEnabled;
    private bool _isInteracting;
    
    #region 生命周期
    
    private void Awake()
    {
        // 自动获取组件（项目标准模式）
        if (communication == null)
            communication = GetComponent<NPCCommunication>();
        
        // 设置初始状态
        _isEnabled = startEnabled;
        
        // 根据启用状态初始化模块
        if (_isEnabled)
        {
            EnableCommunication();
        }
        else
        {
            DisableCommunication();
        }
        
        Debug.Log($"NPC主控初始化完成: {npcName}");
    }
    
    private void Update()
    {
        if (!_isEnabled) return;
        
        // 简单的状态检查
        CheckInteractionState();
    }
    
    #endregion
    
    #region 模块管理（简洁实用）
    
    /// <summary>
    /// 启用NPC交互功能
    /// </summary>
    public void EnableCommunication()
    {
        if (communication != null)
        {
            communication.enabled = true;       //可以对话
            communication.EnableCommunication();//
        }
        _isEnabled = true;
        Debug.Log($"NPC交互已启用: {npcName}");
    }
    
    /// <summary>
    /// 禁用NPC交互功能
    /// </summary>
    public void DisableCommunication()
    {
        if (communication != null)
        {
            communication.enabled = false;
            communication.DisableCommunication();
        }
        _isEnabled = false;
        Debug.Log($"NPC交互已禁用: {npcName}");
    }
    
    /// <summary>
    /// 检查交互状态
    /// </summary>
    private void CheckInteractionState()
    {
        // 这里可以添加简单的状态逻辑
        // 比如：如果正在交互，禁用其他功能
    }
    
    #endregion
    
    #region 公共接口（外部控制）
    
    /// <summary>
    /// 强制开始交互
    /// </summary>
    public void StartInteraction()
    {
        if (!_isEnabled) return;
        
        // 这里可以添加开始交互前的检查
        _isInteracting = true;
        
        // 调用交互模块的方法
        // 注意：原NPCCommunication脚本需要添加ForceStartInteraction方法
        // communication.ForceStartInteraction();
        
        Debug.Log($"强制开始交互: {npcName}");
    }
    
    /// <summary>
    /// 强制结束交互
    /// </summary>
    public void EndInteraction()
    {
        _isInteracting = false;
        
        // 调用交互模块的方法
        // communication.ForceEndInteraction();
        
        Debug.Log($"强制结束交互: {npcName}");
    }
    
    /// <summary>
    /// 设置对话内容
    /// </summary>
    public void SetDialogue(string[] dialogueLines)
    {
        if (communication != null)
        {
            // 原脚本需要添加SetDialogueLines方法
            // communication.SetDialogueLines(dialogueLines);
        }
    }
    
    #endregion
    
    #region 事件处理（可选）
    
    /// <summary>
    /// 处理对话开始事件
    /// </summary>
    public void OnDialogueStart()
    {
        _isInteracting = true;
        // 可以在这里添加交互开始时的逻辑
        Debug.Log($"对话开始: {npcName}");
    }
    
    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    public void OnDialogueEnd()
    {
        _isInteracting = false;
        // 可以在这里添加交互结束时的逻辑
        Debug.Log($"对话结束: {npcName}");
    }
    
    #endregion
    
    #region 属性访问器
    
    public string NPCName => npcName;
    public bool IsEnabled => _isEnabled;
    public bool IsInteracting => _isInteracting;
    
    #endregion
}

/// <summary>
/// 如果需要为原NPCCommunication脚本添加扩展方法，可以在这里定义
/// 但建议保持原脚本的简洁性，只在必要时添加
/// </summary>
public static class NPCCommunicationExtensions
{
    /*
    // 可选：为原脚本添加扩展方法
    public static void ForceStartInteraction(this NPCCommunication comm)
    {
        // 实现强制开始交互的逻辑
    }
    
    public static void ForceEndInteraction(this NPCCommunication comm)
    {
        // 实现强制结束交互的逻辑
    }
    
    public static void SetDialogueLines(this NPCCommunication comm, string[] lines)
    {
        comm.dialogueLines = lines;
    }
    */
}