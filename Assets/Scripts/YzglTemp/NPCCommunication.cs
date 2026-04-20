using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IDialogueTrigger
{
    public List<KeyValuePair<int, string>> GetDialogueLines();
    string[] GetSpeaker();
    public void EndDialogue();
}
/// NPC交互核心脚本：挂载在NPC主体
/// 受NPC主控脚本控制，负责玩家检测、交互触发
public class NPCCommunication : MonoBehaviour, IDialogueTrigger
{
    [Header("检测区域子物体")]
    public GameObject detectArea;   // 由子物体负责玩家检测，主控控制启用/禁用
    [Header("对话内容数组（可编辑）")]
    // public string[] dialogueLines;
    public List<KeyValuePair<int, string>> DialogueLines;
    public string[] speaker;
    [Header("2D UI提示偏移")]
    public Vector2 promptOffset = new Vector2(0, 1.2f);
    [Header("提示词字体")]
    [SerializeField] private TMP_FontAsset ChineseFont;
    [Header("提示词信息")]
    public string interactPromptInfo = "【F 交互】";
    //  动态生成的交互提示
    private GameObject _interactPrompt;
    private Action onPanelReady;

    // 玩家是否在检测范围内
    private bool _isPlayerInRange;
    // 是否正在对话
    private bool _isInDialogue;

    private Canvas _targetCanvas;
    void InitText()
    {
        speaker = new string[] { "NPC", "Riff" };
        DialogueLines = new List<KeyValuePair<int, string>>
        {
            new(0, "TestTestTestTestTestTestTest\nTestTestTestTestTest"),
            new(1, "Test\nTestTest\nTestTestTest\nTest\nTestTestTest\nTestTest")
        };
    }
    private void CreateInteractPrompt()
    {
        // 创建提示物体
        GameObject promptObj = new GameObject("InteractPrompt");
        promptObj.transform.SetParent(_targetCanvas.transform, false);

        // 添加Text
        var text = promptObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = interactPromptInfo;
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.font = ChineseFont;

        // 大小
        var rect = text.rectTransform;
        rect.sizeDelta = new Vector2(160, 40);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        _interactPrompt = promptObj;
    }
#region 对话逻辑 Interface IDialogueTrigger

    public List<KeyValuePair<int, string>> GetDialogueLines() => DialogueLines;
    public string[] GetSpeaker() => speaker;

    /// 开始对话
    private void StartDialogue()
    {
        _isInDialogue = true;
        // 隐藏交互提示
        if (_interactPrompt != null)
            _interactPrompt.SetActive(false);
        // 发布【进入对话】事件 → 玩家主控失活移动/攻击
        var panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Dialogue) as UIPanelDialogue;
        panel.OnDialogue(this);
        EventBus.Instance.Trigger(new DialogueStartEvent(this));
    }
    /// 结束对话（由UI调度器调用）
    public void EndDialogue()
    {
        _isInDialogue = false;
        // 发布【对话结束】事件 → 玩家主控激活移动/攻击
        EventBus.Instance.Trigger<DialogueEndEvent>(new DialogueEndEvent());
    }

#endregion
#region 生命周期
    private void Awake()
    {
        //  自动获取Canvas & 相机
        _targetCanvas = CanvasManager.Instance.TouchCanvas(0);
        CreateInteractPrompt();

        // 初始化：提示默认隐藏
        if (_interactPrompt != null)
            _interactPrompt.SetActive(false);
        
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

    private void LateUpdate()
    {
        if (_interactPrompt == null) return;

        // 世界坐标转屏幕坐标（2D专用）
        Vector3 worldPos = transform.position + new Vector3(promptOffset.x, promptOffset.y, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        _interactPrompt.transform.position = screenPos;
    }
#endregion

    #region 玩家检测触发（由子物体碰撞调用）
    /// 玩家进入检测区域
    public void OnPlayerEnter()
    {
        _isPlayerInRange = true;
        if (!_isInDialogue && _interactPrompt != null)
            _interactPrompt.SetActive(true);
    }

    /// 玩家离开检测区域
    public void OnPlayerExit()
    {
        _isPlayerInRange = false;
        if (_interactPrompt != null)
            _interactPrompt.SetActive(false);
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
        if (_interactPrompt != null)
            _interactPrompt.SetActive(false);
        if (detectArea != null)
            detectArea.SetActive(false);
    }
    #endregion
}