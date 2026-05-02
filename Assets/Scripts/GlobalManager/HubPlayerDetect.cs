using UnityEngine;

/// <summary>
/// Hub 主脑玩家检测脚本：挂载在 DetectPlayer 子物体
/// 首次进入 → 自动启动对话
/// 对话完成后 → 不再自动触发，按 F 键手动对话
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HubPlayerDetect : MonoBehaviour
{
    [SerializeField] private HubDialogueController _dialogueController;
    private bool _isDialogueActive = false;

    private void Awake()
    {
        _dialogueController = GetComponentInParent<HubDialogueController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (HubManager.Instance != null && !HubManager.Instance.hasTalkedToHub && !_isDialogueActive)
        {
            _isDialogueActive = true;
            _dialogueController?.StartDialogue();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            _isDialogueActive = true;
            _dialogueController?.StartDialogue();
        }
    }

    public void ResetDialogueState()
    {
        _isDialogueActive = false;
    }
}