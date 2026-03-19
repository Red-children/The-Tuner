using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class UIComboInfoBar : MonoBehaviour
{
    public Image image;

    //  动画参数

    void Init()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        if (image == null)
        {
            Debug.LogError("UIComboInfo 未找到组件!!!!");
            return;
        }
    }
#region 生命周期
    void Start()
    {
        Init();
    }
    void Update()
    {
        
    }
#endregion
#region 对外接口
    public void SetTargetPercent(float percent)
    {
        
    }
#endregion
}
