using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
#region 数据
    [Header("对话数据")]
    [SerializeField] private DialogueData dialogueData;
    [Header("触发器组件")]
    [SerializeField] private Collider2D trigger;
    [Header("是否冻结玩家")]
    [SerializeField] private bool playfreeze = false;
#endregion

#region 轻量化状态标志

    private bool _isInDialogue = false;
    private bool _triggered = false;

#endregion

    public void OnDialogueEnd(DialogueEndEvent evt)
    {
        _isInDialogue = false;
    }

    public void StartDialogue()
    {
        _isInDialogue = true;
        _triggered = true;
        // 隐藏交互提示
        // 发布【进入对话】事件 → 玩家主控失活移动/攻击
        var panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Echo) as UIPanelEcho;
        panel.OnDialogue(dialogueData);
        EventBus.Instance.Trigger(new DialogueStartEvent(dialogueData, playfreeze));
    }

#region Trigger

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_triggered)
        {
            StartDialogue();
        }
    }

#endregion

#region 生命周期

    void Awake()
    {
        EventBus.Instance.Subscribe<DialogueEndEvent>(OnDialogueEnd);
    }

    void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<DialogueEndEvent>(OnDialogueEnd);
    }
    #endregion
}
