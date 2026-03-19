using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIComboInfoText : MonoBehaviour
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
    void SetDisplayText(string buf)
    {
        text.text = buf;
    }
#endregion
}
