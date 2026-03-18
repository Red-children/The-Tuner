using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIWeaponInfoText : MonoBehaviour
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
            Debug.LogError("UIWeaponInfoText 未找到组件!!!!");
        }
    }

#region 对外接口
    public void SetDisplayText(string buf)
    {
        text.text = buf;
    }
    #endregion
    #region 生命周期
    void Start()
    {
        Init();
    }
    #endregion
}
