using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTheInnerWorld : MonoBehaviour
{
    [Header("加载面板")]
    [SerializeField] private UIPanelLoading panelLoading;
    [Header("切换音乐(如果需要)")]
    [SerializeField] private BGMData newBGMData;
#region 生命周期
    void Awake()
    {
        ChangeBGM();
        Loading();
    }
#endregion
#region 内部操作
    void Loading()
    {
        if (panelLoading == null) return;
        panelLoading.ShowPanel();
    }
    void ChangeBGM()
    {
        if (newBGMData == null) return;
        EventBus.Instance.Trigger<PlayBGMEvent>(new(true, newBGMData));
    }
#endregion
}
