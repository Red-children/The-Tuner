using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡结束触发器
/// 玩家进入触发区域后直接返回主菜单
/// </summary>
public class LevelExit : MonoBehaviour
{
    [Header("触发配置")]
    [Tooltip("是否在进入时立即触发")]
    public bool triggerOnEnter = true;
    
    [Tooltip("是否需要按住按键触发")]
    public bool requireKeyPress = false;
    
    [Tooltip("触发按键")]
    public KeyCode triggerKey = KeyCode.E;

    private bool _playerInRange = false;
    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        _playerInRange = true;
        
        if (triggerOnEnter && !requireKeyPress && !_triggered)
        {
            TriggerLevelEnd();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        _playerInRange = false;
    }

    private void Update()
    {
        if (_playerInRange && requireKeyPress && !_triggered && Input.GetKeyDown(triggerKey))
        {
            TriggerLevelEnd();
        }
    }

    private void TriggerLevelEnd()
    {
        _triggered = true;
        
        // 直接加载主菜单场景
        SceneManager.LoadScene("MainMenu");
    }
}