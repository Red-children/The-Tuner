using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

//  控制血条上的文字
public class UIHPText : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Init()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }
        if (text == null)
        {
            Debug.LogError($"UIHPText 未找到组件!!!!");
            return;
        }
        text.text = "Test";
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
        text.SetAllDirty();
    }
#endregion
}
