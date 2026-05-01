using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景加载后自动返回触发器
/// 场景加载后自动开始对话 → 对话结束后切换到目标场景
/// 用于表世界这类过渡场景，无需玩家交互
/// </summary>
public class SceneReturnTrigger : MonoBehaviour
{
    [Header("对话数据")]
    [SerializeField] private DialogueData dialogueData;

    [Header("目标场景")]
    [SerializeField] private string targetSceneName;

    [Header("目标场景中的出生点")]
    [SerializeField] private string targetSpawnPointName;

    [Header("是否冻结玩家")]
    [SerializeField] private bool freezePlayer = true;

    private bool _isInDialogue = false;

    void Start()
    {
        EventBus.Instance.Subscribe<DialogueEndEvent>(OnDialogueEnd);
        StartCoroutine(DelayedStartDialogue());
    }

    private System.Collections.IEnumerator DelayedStartDialogue()
    {
        yield return null;
        StartReturnDialogue();
    }

    void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<DialogueEndEvent>(OnDialogueEnd);
    }

    private void StartReturnDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("SceneReturnTrigger: 未设置对话数据");
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("SceneReturnTrigger: 未设置目标场景名");
            return;
        }

        _isInDialogue = true;

        var panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Echo) as UIPanelEcho;
        if (panel == null)
        {
            Debug.LogWarning("SceneReturnTrigger: 无法打开Echo面板，跳过对话，直接返回目标场景");
            SceneManager.LoadScene(targetSceneName);
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

        if (!string.IsNullOrEmpty(targetSpawnPointName))
        {
            PlayerSpawnInfo.spawnPointName = targetSpawnPointName;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}