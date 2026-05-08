using UnityEngine;

public class ESCHandler : MonoBehaviour
{
    public UIPanelPause panel;
    private bool _isPanelOpen = false;
    private float _previousTimeScale = 1f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        EnsurePanelExists();
    }

    void Update()
    {
        if (_isPanelOpen) return;
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 操作前确保面板存在
            EnsurePanelExists();
            
            _isPanelOpen = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            
            panel.RegisterOnCloseComplete(OnPanelClosed);
            panel.ShowPanel();
        }
    }

    private void OnPanelClosed()
    {
        _isPanelOpen = false;
        Time.timeScale = _previousTimeScale;
    }

    /// <summary>
    /// 确保面板存在，如果被销毁则重新创建
    /// </summary>
    private void EnsurePanelExists()
    {
        // 如果 panel 为 null 或者对应的 GameObject 已被销毁
        if (panel == null || panel.gameObject == null)
        {
            Debug.Log("Pause panel destroyed, recreating...");
            panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Pause, false) as UIPanelPause;
        }
    }
}