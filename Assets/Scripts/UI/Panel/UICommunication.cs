using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

/// <summary>
/// 对话UI显示脚本：DOTween + UGUI Text 版
/// </summary>
public class UICommunication : MonoBehaviour
{
    [Header("对话文本")]
    [SerializeField] private Text[] dialogueTexts;  // 主文本框组件
    [SerializeField] private Text[] speakerTexts;   // 名字文本框组件

    [Header("文字速度")]
    public float textSpeed = 0.05f;
    private List<KeyValuePair<int, string>> _currentLines;
    private int _currentLineIndex;
    private string[] _speaker;
    private bool _isTyping; //  是否正在打字动画

    private Tweener _textTweener;
    // 获取自己父物体的面板脚本
    [SerializeField] private UIPanelDialogue dialoguePanel;
#region 生命周期
    private void Awake()
    {
        if (dialogueTexts == null)
        {
            Debug.Log("UICommunication 组件缺失");
        }
        // 自动获取自己归属的对话面板（非单例）
        dialoguePanel = GetComponent<UIPanelDialogue>();

        _speaker = new string[2] {null, null};
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryNextDialogue();
        }
    }
    private void OnDestroy()
    {
        KillCurrentTween();
    }
#endregion
    private void TryNextDialogue()
    {
        if (_currentLines == null || dialogueTexts == null) return;

        if (_isTyping)
        {
            ShowFullLine();
        }
        else
        {
            NextLine();
        }
    }

    #region 对话核心

    public void SetSpeakers(string[] speakers)
    {
        _speaker = speakers;
    }

    public void StartDialogue(List<KeyValuePair<int, string>> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("对话内容为空");
            return;
        }

        KillCurrentTween();

        _currentLines = lines;
        _currentLineIndex = 0;
        TypeLine();
    }

    private void TypeLine()
    {
        KeyValuePair<int, string> currentText = _currentLines[_currentLineIndex];

        if (_speaker[currentText.Key] != null) 
            speakerTexts[currentText.Key].text = _speaker[currentText.Key];

        dialogueTexts[currentText.Key].text = string.Empty;
        _isTyping = true;
        _textTweener = dialogueTexts[currentText.Key].DOText(currentText.Value, currentText.Value.Length * textSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _isTyping = false;
            });
    }

    private void ShowFullLine()
    {
        KillCurrentTween();
        var currentLine = _currentLines[_currentLineIndex];
        dialogueTexts[currentLine.Key].text = _currentLines[_currentLineIndex].Value;
        _isTyping = false;
    }

    private void NextLine()
    {
        _currentLineIndex++;

        if (_currentLineIndex < _currentLines.Count)
        {
            TypeLine();
        }
        else
        {
            if (dialoguePanel != null)
            Invoke(nameof(CloseDialogueSafe), 0.01f);
        }
    }
    private void CloseDialogueSafe()
    {
        UIManager.Instance.ClosePanel(UIManager.UIConst.Dialogue);
    }
    private void KillCurrentTween()
    {
        if (_textTweener != null && _textTweener.IsActive())
        {
            _textTweener.Kill();
        }
    }
    #endregion
}