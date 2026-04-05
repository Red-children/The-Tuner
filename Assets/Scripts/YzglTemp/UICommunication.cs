using UnityEngine;
using TMPro;

/// <summary>
/// 对话UI显示脚本：挂载在DialoguePanel上
/// 处理文本推进、显示逻辑
/// </summary>
public class UICommunication : MonoBehaviour
{
    [Header("对话文本")]
    public TextMeshProUGUI dialogueText;
    [Header("对话推进间隔")]
    public float textSpeed = 0.05f;

    private string[] _currentLines;
    private int _currentLineIndex;
    private bool _isTyping; // 是否正在打字

    private void Awake()
    {
        dialogueText.text = string.Empty;
    }

    private void Update()
    {
        // 鼠标左键推进对话
        if (Input.GetMouseButtonDown(0))
        {
            if (_isTyping)
            {
                // 正在打字 → 直接显示完整句子
                StopAllCoroutines();
                dialogueText.text = _currentLines[_currentLineIndex];
                _isTyping = false;
            }
            else
            {
                // 显示下一句
                NextLine();
            }
        }
    }

    #region 对话控制
    /// <summary>
    /// 启动对话
    /// </summary>
    public void StartDialogue(string[] lines)
    {
        _currentLines = lines;
        _currentLineIndex = 0;
        StartTyping();
    }

    /// <summary>
    /// 开始打字显示
    /// </summary>
    private void StartTyping()
    {
        _isTyping = true;
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    /// <summary>
    /// 打字效果协程
    /// </summary>
    private System.Collections.IEnumerator TypeLine()
    {
        dialogueText.text = string.Empty;
        foreach (char c in _currentLines[_currentLineIndex].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        _isTyping = false;
    }

    /// <summary>
    /// 下一句对话
    /// </summary>
    private void NextLine()
    {
        _currentLineIndex++;
        if (_currentLineIndex < _currentLines.Length)
        {
            StartTyping();
        }
        else
        {
            // 对话结束 → 隐藏UI
            UIDialogueDispatcher.Instance.HideDialogue();
        }
    }
    #endregion
}