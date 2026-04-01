using UnityEngine;

/// NPC交互核心脚本：挂载在NPC主体
/// 受NPC主控脚本控制，负责玩家检测、交互触发
public class NPCCommunication : MonoBehaviour
{
    [Header("检测区域子物体")]
    public GameObject detectArea;
    [Header("交互提示文本")]
    public GameObject interactPrompt;
    [Header("对话内容数组（可编辑）")]
    public string[] dialogueLines;

    // 玩家是否在检测范围内
    private bool _isPlayerInRange;
    // 是否正在对话
    private bool _isInDialogue;

    void InitText()
    {
        dialogueLines = new string[2];
        dialogueLines[0] = "TestTestTestTestTestTestTest\nTestTestTestTestTest";
        dialogueLines[1] = "Test\nTestTest\nTestTestTest\nTest\nTestTestTest\nTestTest";
    }
    private void Awake()
    {
        // 初始化：提示默认隐藏
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
        
        // 启用检测区域（由主控控制开关）
        if (detectArea != null)
            detectArea.SetActive(true);

        InitText();
    }

    private void Update()
    {
        // 受主控控制：如果NPC未启用交互，直接返回
        if (!enabled) return;
        if (_isInDialogue) return;

        // 玩家在范围内 + 按下F键 → 启动对话
        if (_isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Player Pressed F");
            StartDialogue();
        }
    }

#region 玩家检测触发（由子物体碰撞调用）
    /// 玩家进入检测区域
    public void OnPlayerEnter()
    {
        _isPlayerInRange = true;
        if (!_isInDialogue && interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    /// 玩家离开检测区域
    public void OnPlayerExit()
    {
        _isPlayerInRange = false;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }
#endregion
#region 对话逻辑
    /// 开始对话
    private void StartDialogue()
    {
        _isInDialogue = true;
        // 隐藏交互提示
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
        // 发布【进入对话】事件 → 玩家主控失活移动/攻击
        EventBus.Instance.Trigger<DialogueStartEvent>(new DialogueStartEvent());
        //  TODO:
        // 调度UI显示对话
        UIDialogueDispatcher.Instance.ShowDialogue(dialogueLines);
    }
    /// 结束对话（由UI调度器调用）
    public void EndDialogue()
    {
        _isInDialogue = false;
        // 发布【对话结束】事件 → 玩家主控激活移动/攻击
        EventBus.Instance.Trigger<DialogueEndEvent>(new DialogueEndEvent());
    }
    #endregion

    #region 供NPC主控脚本调用的接口
    /// <summary>
    /// 启用NPC交互功能
    /// </summary>
    public void EnableCommunication()
    {
        enabled = true;
        if (detectArea != null)
            detectArea.SetActive(true);
    }

    /// <summary>
    /// 禁用NPC交互功能
    /// </summary>
    public void DisableCommunication()
    {
        enabled = false;
        _isPlayerInRange = false;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
        if (detectArea != null)
            detectArea.SetActive(false);
    }
    #endregion
}