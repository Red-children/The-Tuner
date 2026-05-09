using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 碰撞体对话触发器（类似Hub）
/// 玩家进入触发区域后触发对话，每个触发器只触发一次
/// 对话结束后可选择传送玩家到指定位置
/// </summary>
public class AutoDialogueTrigger : MonoBehaviour
{
    [Header("对话配置")]
    [Tooltip("对话数据，在Inspector中配置")]
    [SerializeField] private DialogueData dialogueData;
    
    [Tooltip("是否冻结玩家")]
    [SerializeField] private bool freezePlayer = false;
    
    [Tooltip("触发器唯一ID，用于区分不同触发器")]
    [SerializeField] private string triggerId = "default";

    [Header("传送配置")]
    [Tooltip("对话结束后传送的目标位置（不设置则不传送）")]
    [SerializeField] private Transform teleportTarget;

    private UIPanelEcho _panel;
    private bool _isInDialogue = false;
    private static readonly HashSet<string> _triggeredIds = new HashSet<string>();

    private void StartDialogue()
    {
        if (_isInDialogue || _triggeredIds.Contains(triggerId)) return;
        
        _isInDialogue = true;
        _triggeredIds.Add(triggerId);

        // 打开对话面板
        _panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Echo) as UIPanelEcho;
        if (_panel != null)
        {
            _panel.RegisterOnCloseComplete(EndDialogue);
            _panel.OnDialogue(dialogueData);
            // 发布进入对话事件，通知其他系统
            EventBus.Instance.Trigger(new DialogueStartEvent(dialogueData, freezePlayer));
        }
        else
        {
            Debug.LogWarning("无法打开对话面板");
            _isInDialogue = false;
            _triggeredIds.Remove(triggerId);
        }
    }

    private void EndDialogue()
    {
        if (_panel != null)
        {
            _panel.UnregisterOnCloseComplete(EndDialogue);
        }
        _isInDialogue = false;

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
            Debug.LogWarning("AutoDialogueTrigger: 未找到玩家对象");
            return;
        }

        // 打开加载界面
        UIManager.Instance.OpenPanel(UIManager.UIConst.Loading);

        // 执行传送
        player.transform.position = teleportTarget.position;
        player.transform.rotation = teleportTarget.rotation;

        Debug.Log($"玩家已传送到: {teleportTarget.position}");

        // 延迟关闭加载界面（2秒后自动关闭）
        Invoke(nameof(CloseLoadingPanel), 2f);
    }

    private void CloseLoadingPanel()
    {
        UIManager.Instance.ClosePanel(UIManager.UIConst.Loading);
    }

    private void OnDialogueEnd(DialogueEndEvent evt)
    {
        if (_panel != null)
        {
            UIManager.Instance.ClosePanel(_panel);
        }
    }

    #region 碰撞体触发
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_triggeredIds.Contains(triggerId) && !_isInDialogue)
        {
            StartDialogue();
        }
    }
    #endregion

        private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_triggeredIds.Contains(triggerId) && !_isInDialogue)
        {
            StartDialogue();
        }
    }


    #region 生命周期
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EventBus.Instance.Subscribe<DialogueEndEvent>(OnDialogueEnd);
    }

    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<DialogueEndEvent>(OnDialogueEnd);
        
        if (_panel != null)
        {
            _panel.UnregisterOnCloseComplete(EndDialogue);
        }
    }
    #endregion
}