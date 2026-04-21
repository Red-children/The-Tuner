using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
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
    // private List<KeyValuePair<int, string>> _currentLines;
    private DialogueLines _currentLines;
    private int _currentLineIndex;
    private string[] _speaker;
    private bool _isTyping; //  是否正在打字动画

    private Tweener _textTweener;
    // [SerializeField] private UIPanelDialogue dialoguePanel;
#region 生命周期
    private void Awake()
    {
        if (dialogueTexts == null)
        {
            Debug.Log("UICommunication 组件缺失");
        }
        // 自动获取自己归属的对话面板
        // dialoguePanel = GetComponent<UIPanelDialogue>();

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
        if (_currentLines.entries == null || dialogueTexts == null) return;

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

    public void StartDialogue(DialogueLines lines)
    {
        if (lines.entries == null || lines.entries.Count == 0)
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
        LinesEntry currentText = _currentLines.entries[_currentLineIndex];

        if (_speaker[currentText.id] != null) 
            speakerTexts[currentText.id].text = _speaker[currentText.id];

        dialogueTexts[currentText.id].text = string.Empty;
        _isTyping = true;
        _textTweener = dialogueTexts[currentText.id].DOText(currentText.line, currentText.line.Length * textSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _isTyping = false;
            });
    }

    private void ShowFullLine()
    {
        KillCurrentTween();
        LinesEntry currentLine = _currentLines.entries[_currentLineIndex];
        dialogueTexts[currentLine.id].text = _currentLines.entries[_currentLineIndex].line;
        _isTyping = false;
    }

    private void NextLine()
    {
        _currentLineIndex++;

        if (_currentLineIndex < _currentLines.entries.Count)
        {
            TypeLine();
        }
        else
        {
            EventBus.Instance.Trigger(new DialogueEndEvent());
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
}