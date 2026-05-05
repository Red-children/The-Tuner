using UnityEngine;

/// <summary>
/// 预加载UI, 不显示
/// </summary>
public class UILoader : MonoBehaviour
{
    [Header("加载标志")]
    [Tooltip("战斗")]
    public bool Battle = false;
    private UIPanelinBattle panelinBattle;
    [Tooltip("暂停")]
    public bool Pause = false;
    private UIPanelPause panelPause;
    // [Tooltip("加载(暂无)")]
    // public bool Load = false;
#region 读取引用
    public UIPanelinBattle GetPanelinBattle() => panelinBattle;
    public UIPanelPause GetPanelPause() => panelPause;
#endregion
#region 初始化
    void OnInitComplete(InitComplete evt)
    {
        Init();
    }
    void Init()
    {
        if (Battle)
            panelinBattle = UIManager.Instance.OpenPanel(UIManager.UIConst.Battle, false) as UIPanelinBattle;
        if (Pause)
            panelPause = UIManager.Instance.OpenPanel(UIManager.UIConst.Pause, false) as UIPanelPause;
    }
#endregion
#region 生命周期
    void Awake()
    {
        EventBus.Instance.Subscribe<InitComplete>(OnInitComplete);
    }
    void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<InitComplete>(OnInitComplete);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ESCEvent();
        } 
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            ZEvent();
        }
    }
#endregion
#region ESC 事件处理
    private void ESCEvent()
    {
        if (panelPause.gameObject.activeSelf)
        {
            panelPause.HidePanel();
        }
        else 
        {
            Time.timeScale = 0f;
            panelPause.RegisterOnCloseComplete(() =>
            {
                Time.timeScale = 1f;
            });
            panelPause.ShowPanel();
        }
    }
#endregion
#region Z 事件处理
    private void ZEvent()
    {
        if (panelinBattle.gameObject.activeSelf)
            panelinBattle.HidePanel();
        else panelinBattle.ShowPanel();
    }
#endregion
}
