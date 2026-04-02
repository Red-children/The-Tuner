using UnityEngine;

/// 玩家检测脚本：挂载在NPC的DetectPlayer子物体
[RequireComponent(typeof(Collider2D))]
public class PlayerDetect : MonoBehaviour
{
    [SerializeField] private NPCCommunication _npcCommunication;

    private void Awake()
    {
        // 获取父物体的NPC脚本
        _npcCommunication = GetComponentInParent<NPCCommunication>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测到玩家（请确保玩家Tag为Player）
        Debug.Log("检测到玩家");
        if (other.CompareTag("Player"))
        {
            _npcCommunication?.OnPlayerEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _npcCommunication?.OnPlayerExit();
        }
    }
}