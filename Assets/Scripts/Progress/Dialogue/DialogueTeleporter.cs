using UnityEngine;

/// <summary>
/// 对话传送触发器：碰撞触发对话，对话结束后传送玩家
/// 如果未设置传送目标，则对话结束后不传送
/// </summary>
public class DialogueTeleporter : MonoBehaviour
{
    [Header("对话数据")]
    [Tooltip("对话数据资产")]
    public DialogueData dialogueData;

    [Header("传送目标")]
    [Tooltip("对话结束后传送的目标位置（不设置则不传送）")]
    public Transform teleportTarget;

    [Header("对话设置")]
    [Tooltip("是否冻结玩家（禁止移动/攻击）")]
    public bool freezePlayer = true;

    [Header("触发设置")]
    [Tooltip("是否只触发一次")]
    public bool triggerOnlyOnce = true;

    private bool _triggered = false;
    private bool _isInDialogue = false;

    private void Awake()
    {
        EventBus.Instance.Subscribe<DialogueEndEvent>(OnDialogueEnd);
    }

    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<DialogueEndEvent>(OnDialogueEnd);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnlyOnce && _triggered)
            return;

        if (_isInDialogue)
            return;

        StartDialogue();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnlyOnce && _triggered)
            return;

        if (_isInDialogue)
            return;

        StartDialogue();
    }

    private void StartDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("DialogueTeleporter: 对话数据未设置");
            return;
        }

        _isInDialogue = true;
        _triggered = true;

        var panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Echo) as UIPanelEcho;
        if (panel == null)
        {
            Debug.LogWarning("DialogueTeleporter: 无法打开Echo面板");
            _isInDialogue = false;
            return;
        }

        panel.OnDialogue(dialogueData);
        EventBus.Instance.Trigger(new DialogueStartEvent(dialogueData, freezePlayer));
    }

    private void OnDialogueEnd(DialogueEndEvent evt)
    {
        if (!_isInDialogue)
            return;

        _isInDialogue = false;
        UIManager.Instance.ClosePanel(UIManager.UIConst.Echo);

        // 只有设置了传送目标才传送
        if (teleportTarget != null)
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("DialogueTeleporter: 未找到玩家对象");
            return;
        }

        player.transform.position = teleportTarget.position;
        player.transform.rotation = teleportTarget.rotation;

        Debug.Log($"玩家已传送到: {teleportTarget.position}");
    }
}