using UnityEngine;

/// <summary>
/// 对话UI调度器：单例，统一管理对话显示/隐藏
/// 挂载在DialoguePanel上
/// </summary>
public class UIPanelDialogue : UIBasePanel
{
    // 单例
    [Header("对话UI脚本")]
    public UICommunication uiCommunication;
    [Header("归属的NPC")]
    public NPCCommunication currentNPC;     

    private void Awake()
    {
        // 初始化隐藏面板
        gameObject.SetActive(false);
    }
    /// <summary>
    /// 绑定发起对话的NPC
    /// </summary>
    public void BindNPC(NPCCommunication npc)
    {
        currentNPC = npc;
    }
    /// <summary>
    /// 显示对话UI
    /// </summary>
    public void ShowDialogue(string[] lines)
    {
        gameObject.SetActive(true);
        uiCommunication.StartDialogue(lines);
    }

    /// <summary>
    /// 隐藏对话UI（对话结束时调用）
    /// </summary>
    public void HideDialogue()
    {
        gameObject.SetActive(false);
        // 通知NPC结束对话
        currentNPC?.EndDialogue();
        DialogueManager.Instance.EndDialogue();
    }
}