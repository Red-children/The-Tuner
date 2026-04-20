using UnityEngine;

/// <summary>
/// 对话管理器：单例
/// 负责接收事件 → 打开对话面板 → 传递数据
/// </summary>
public class DialogueManager
{
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            _instance ??= new DialogueManager();
            return _instance;
        }
    }

    private UIPanelDialogue _currentDialoguePanel;

    /// <summary>
    /// 外部调用：启动对话
    /// </summary>
    public void StartDialogue(NPCCommunication npc, string[] lines)
    {
        // 1. 打开对话面板（自动挂到 Canvas_System 或 Canvas_Main）
        _currentDialoguePanel = UIManager.Instance.OpenPanel(UIManager.UIConst.Dialogue) as UIPanelDialogue;

        if (_currentDialoguePanel == null)
        {
            Debug.LogError("对话面板加载失败");
            return;
        }

        // 2. 绑定NPC + 显示内容
        _currentDialoguePanel.BindNPC(npc);
        _currentDialoguePanel.ShowDialogue(lines);
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        if (_currentDialoguePanel != null)
        {
            UIManager.Instance.ClosePanel(UIManager.UIConst.Dialogue);
            _currentDialoguePanel = null;
        }
    }
}