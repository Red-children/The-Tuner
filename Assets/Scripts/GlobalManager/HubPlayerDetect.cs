    using UnityEngine;

/// <summary>
/// Hub 主脑玩家检测脚本：挂载在 DetectPlayer 子物体
/// 玩家进入 → 启动主脑对话，玩家离开 → 无操作
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HubPlayerDetect : MonoBehaviour
{
    [SerializeField] private HubDialogueController _dialogueController;

    private void Awake()
    {
        _dialogueController = GetComponentInParent<HubDialogueController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _dialogueController?.StartDialogue();
        }
    }
}