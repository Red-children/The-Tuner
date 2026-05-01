using UnityEngine;

/// <summary>
/// Hub 主脑总控制器
/// 管理主脑的启用/禁用、名字显示、对话模块的生命周期
/// </summary>
public class HubController : MonoBehaviour
{
    [Header("主脑基本信息")]
    [SerializeField] private string hubName = "主脑";
    [SerializeField] private bool startEnabled = true;

    [Header("模块引用")]
    [SerializeField] private HubDialogueController dialogueController;

    [Header("名字UI")]
    [SerializeField] private NPCNameUI nameUI;

    private bool _isEnabled;

    private void Awake()
    {
        if (dialogueController == null)
            dialogueController = GetComponent<HubDialogueController>();

        _isEnabled = startEnabled;

        if (nameUI != null)
            nameUI.Init(transform, hubName);

        if (_isEnabled)
            EnableHub();
        else
            DisableHub();
    }

    public void EnableHub()
    {
        if (dialogueController != null)
            dialogueController.enabled = true;

        _isEnabled = true;

        if (nameUI != null)
            nameUI.SetVisible(true);
    }

    public void DisableHub()
    {
        if (dialogueController != null)
            dialogueController.enabled = false;

        _isEnabled = false;

        if (nameUI != null)
            nameUI.SetVisible(false);
    }

    private void OnDestroy()
    {
        if (nameUI != null)
            nameUI.DestroyName();
    }
}
