using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadTheInnerWorld : MonoBehaviour
{
    [Header("加载面板")]
    [SerializeField] private UIPanelLoading panelLoading;
    [Header("切换音乐(如果需要)")]
    [SerializeField] private BGMData newBGMData;
    [SerializeField] private bool crosshair;
#region 生命周期
    void Awake()
    {
        ChangeBGM();
        Loading();
        Crosshair();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
#endregion
#region 主动加载
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ChangeBGM();
        Loading();
        Crosshair();
    }
#endregion
#region 内部操作
    void Loading()
    {
            panelLoading = UIManager.Instance.GetPanel(UIManager.UIConst.Loading) as UIPanelLoading;
        if (panelLoading == null)
            panelLoading = UIManager.Instance.OpenPanel(UIManager.UIConst.Loading) as UIPanelLoading;
        else panelLoading.ShowPanel();
    }
    void ChangeBGM()
    {
        if (newBGMData == null) return;
        EventBus.Instance.Trigger<PlayBGMEvent>(new(true, newBGMData));
    }
    void Crosshair()
    {
        if (!crosshair) return;
        var uiCrosshair = UIManager.Instance.GetPanel(UIManager.UIConst.Crosshair) as UICrosshair;
        if (uiCrosshair == null)
            UIManager.Instance.OpenPanel(UIManager.UIConst.Crosshair);
        uiCrosshair.RegisterOnOpenComplete(() =>
        {
           Invoke(nameof(CompleteLoading), 0.5f); 
        });
    }
    void CompleteLoading()
    {
        var panel = UIManager.Instance.GetPanel(UIManager.UIConst.Loading) as UIPanelLoading;
        panel.Complete(false);
    }
#endregion
}
