using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 对话UI显示脚本：DOTween + UGUI Text 版
/// </summary>
public class UICommunication : MonoBehaviour
{
    [Header("对话文本")]
    [SerializeField] private Text dialogueText;

    [Header("文字速度")]
    public float textSpeed = 0.05f;

    private string[] _currentLines;
    private int _currentLineIndex;
    private bool _isTyping; //  是否正在打字动画

    private Tweener _textTweener;

    private void Awake()
    {
        if (dialogueText != null)
            dialogueText.text = string.Empty;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryNextDialogue();
        }
    }

    private void TryNextDialogue()
    {
        if (_currentLines == null || dialogueText == null) return;

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
    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
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
        string currentText = _currentLines[_currentLineIndex];
        dialogueText.text = string.Empty;
        _isTyping = true;

        _textTweener = dialogueText.DOText(currentText, currentText.Length * textSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _isTyping = false;
            });
    }

    private void ShowFullLine()
    {
        KillCurrentTween();
        dialogueText.text = _currentLines[_currentLineIndex];
        _isTyping = false;
    }

    private void NextLine()
    {
        _currentLineIndex++;

        if (_currentLineIndex < _currentLines.Length)
        {
            TypeLine();
        }
        else
        {
            UIPanelDialogue.Instance.HideDialogue();
        }
    }

    private void KillCurrentTween()
    {
        if (_textTweener != null && _textTweener.IsActive())
        {
            _textTweener.Kill();
        }
    }
    #endregion

    private void OnDestroy()
    {
        KillCurrentTween();
    }
}