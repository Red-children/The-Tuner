using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// 对话UI显示脚本：DOTween + UGUI Text 版
/// </summary>
[RequireComponent(typeof(UISoundPlayer))]
public class UICommunication : MonoBehaviour
{
    [Header("音效组件")]
    [Tooltip("一次性音效脚本")]
    [SerializeField] private UISoundPlayer soundPlayer;
    [SerializeField] private AudioSource[] sourceOnspeak;   // 循环打字音效
    [SerializeField] private AudioClip clipOnStart;         //  音效 对话开始
    [SerializeField] private AudioClip clipOnNext;          //  音效 下一句
    [Header("对话文本")]
    [SerializeField] private Text[] dialogueTexts;  // 主文本框组件
    [SerializeField] private Text[] speakerTexts;   // 名字文本框组件

    [Header("文字速度")]
    public float textSpeed = 0.05f;
    private DialogueLines _currentLines;
    private int _currentLineIndex;
    private string[] _speaker;
    private bool _isTyping; //  是否正在打字动画

    private Tweener _textTweener;
#region 生命周期
    private void Awake()
    {
        if (soundPlayer == null) 
            soundPlayer = GetComponent<UISoundPlayer>();
        if (dialogueTexts == null)
        {
            Debug.Log("UICommunication 组件缺失");
        }

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
        soundPlayer.PlaySoundManually(clipOnStart);
        TypeLine();
    }

    private void TypeLine()
    {
        LinesEntry currentText = _currentLines.entries[_currentLineIndex];

        if (_speaker[currentText.id] != null) 
            speakerTexts[currentText.id].text = _speaker[currentText.id];

        dialogueTexts[currentText.id].text = string.Empty;
        _isTyping = true;
        _textTweener = dialogueTexts[currentText.id].DOText(currentText.line, currentText.line.Length * textSpeed, true)
            .SetEase(Ease.Linear)
            .OnStart(() =>
            {
                soundPlayer.PlaySoundManually(clipOnNext);
                sourceOnspeak[currentText.id].loop = true;
                sourceOnspeak[currentText.id].Play();
            })
            .OnComplete(() =>
            {
                sourceOnspeak[currentText.id].Stop();
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
            _textTweener.Complete();
            _textTweener.Kill();
        }
    }
    #endregion
}