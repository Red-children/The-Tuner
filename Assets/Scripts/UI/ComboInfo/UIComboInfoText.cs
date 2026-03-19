using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIComboInfoText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Animator animator;
    //  Rank - Animation
    private Dictionary<RhythmRank, string> _animationDict;
    void Init()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (text == null || animator == null)
        {
            Debug.LogError("UIComboInfoText 未找到组件");
            return;
        }
    }
#region 生命周期
    void Start()
    {
        Init();
    }
#endregion
#region 对外接口
    public void SetDisplayText(string buf)
    {
        text.text = buf;
        Debug.Log($"UIComboInfoText Set Display Text {buf}");
    }
    public void TextAnimation(RhythmRank rank)
    {
        //  TODO:Miss || Good || Great || Perfect 时字体的动画
        //  Make sure animation in the dictionary
        animator.Play(_animationDict[rank]);
        Debug.Log($"UIComboInfoText Play Animation {rank}");
    }
#endregion
}
