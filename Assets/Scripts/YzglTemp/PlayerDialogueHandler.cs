using System;
using UnityEngine;

/// <summary>
/// 玩家对话事件处理脚本：挂载在玩家对象上
/// 处理对话开始/结束时的玩家控制状态
/// </summary>
public class PlayerDialogueHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    private PlayerDash _playerDash;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        // 获取玩家控制组件
        _playerController = GetComponent<PlayerController>();
        _playerAttack = GetComponent<PlayerAttack>();
        _playerDash = GetComponent<PlayerDash>();
        _playerMovement = GetComponent<PlayerMovement>();

        // 订阅对话事件
        EventBus.Instance.Subscribe<DialogueStartEvent>(OnDialogueStart);
        EventBus.Instance.Subscribe<DialogueEndEvent>(OnDialogueEnd);
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<DialogueStartEvent>(OnDialogueStart);
            EventBus.Instance.Unsubscribe<DialogueEndEvent>(OnDialogueEnd);
        }
    }

    /// <summary>
    /// 对话开始时禁用玩家控制
    /// </summary>
    private void OnDialogueStart(DialogueStartEvent evt)
    {
        Debug.Log("对话开始 - 禁用玩家控制");
        
        if (_playerController != null)
            _playerController.enabled = false;
        
        if (_playerAttack != null)
            _playerAttack.enabled = false;
        
        if (_playerDash != null)
            _playerDash.enabled = false;
        if(_playerMovement != null)
            _playerMovement.enabled = false;
    }

    /// <summary>
    /// 对话结束时启用玩家控制
    /// </summary>
    private void OnDialogueEnd(DialogueEndEvent evt)
    {
        Debug.Log("对话结束 - 启用玩家控制");
        
        if (_playerController != null)
            _playerController.enabled = true;
        
        if (_playerAttack != null)
            _playerAttack.enabled = true;
        
        if (_playerDash != null)
            _playerDash.enabled = true;
        if (_playerMovement != null)
            _playerMovement.enabled = true;
    }
}