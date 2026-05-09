using UnityEngine;

/// <summary>
/// Hub 主脑专用对话控制器
/// 玩家进入触发区域 → 自动开始对话 → 对话结束后传送玩家到目标位置
/// 仅用于 Hub 场景，不与其他 NPC 共享
/// </summary>
public class HubDialogueController : MonoBehaviour
{
    [Header("检测区域子物体")]
    public GameObject detectArea;

    [Header("对话数据")]
    [SerializeField] private DialogueData dialogueData;

    [Header("是否冻结玩家")]
    [SerializeField] private bool freezePlayer = true;

    private bool _isInDialogue = false;
    private bool _hasTriggered = false;

    void Awake()
    {
        EventBus.Instance.Subscribe<DialogueEndEvent>(OnDialogueEnd);

        if (detectArea != null)
            detectArea.SetActive(true);
    }

    void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<DialogueEndEvent>(OnDialogueEnd);
    }

    public void StartDialogue()
    {
        //判空检测
        if (_isInDialogue)
            return;

        if (dialogueData == null)
        {
            Debug.LogWarning("HubDialogueController: 未设置对话数据");
            ResetHubPlayerDetectState();
            return;
        }

        var panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Echo) as UIPanelEcho;
        if (panel == null)
        {
            Debug.LogWarning("HubDialogueController: 无法打开Echo面板");
            ResetHubPlayerDetectState();
            return;
        }

        _isInDialogue = true;
        _hasTriggered = true;
        panel.OnDialogue(dialogueData);
        EventBus.Instance.Trigger(new DialogueStartEvent(dialogueData, freezePlayer));
    }

    private void ResetHubPlayerDetectState()
    {
        if (detectArea != null)
        {
            var detect = detectArea.GetComponent<HubPlayerDetect>();
            if (detect != null)
                detect.ResetDialogueState();
        }
    }

    private void OnDialogueEnd(DialogueEndEvent evt)
    {
        if (!_isInDialogue)
            return;

        _isInDialogue = false;

        ResetHubPlayerDetectState();

        bool isFirstDialogue = HubManager.Instance != null && !HubManager.Instance.hasTalkedToHub;

        if (isFirstDialogue)
        {
            HubManager.Instance.hasTalkedToHub = true;
            UIManager.Instance.ClosePanel(UIManager.UIConst.Echo);

            // 传送玩家到目标位置
            HubManager.Instance.TeleportToTarget();
        }
        else
        {
            UIManager.Instance.ClosePanel(UIManager.UIConst.Echo);
        }
    }
}