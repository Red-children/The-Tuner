using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Hub 主脑专用对话控制器
/// 玩家进入触发区域 → 自动开始对话 → 对话结束后切换到战斗场景
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
        if (dialogueData == null)
        {
            Debug.LogWarning("HubDialogueController: 未设置对话数据");
            return;
        }

        _isInDialogue = true;
        _hasTriggered = true;

        var panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Echo) as UIPanelEcho;
        panel.OnDialogue(dialogueData);
        EventBus.Instance.Trigger(new DialogueStartEvent(dialogueData, freezePlayer));
    }

    private void OnDialogueEnd(DialogueEndEvent evt)
    {
        if (!_isInDialogue)
            return;

        _isInDialogue = false;

        UIManager.Instance.ClosePanel(UIManager.UIConst.Echo);

        if (HubManager.Instance == null)
        {
            Debug.LogWarning("HubDialogueController: HubManager 不存在");
            return;
        }

        var target = HubManager.Instance.GetCurrentTarget();
        if (target == null || string.IsNullOrEmpty(target.sceneName))
        {
            Debug.LogWarning("HubDialogueController: 当前章节无目标场景");
            return;
        }

        PlayerSpawnInfo.spawnPointName = target.spawnPointName;
        SceneManager.LoadScene(target.sceneName);
    }
}
