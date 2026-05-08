using UnityEngine;

public class ESCHandler : MonoBehaviour
{
    public UIPanelPause panel;
    private bool _isPanelOpen = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        panel = UIManager.Instance.OpenPanel(UIManager.UIConst.Pause, false) as UIPanelPause;
    }

    void Update()
    {
        if (_isPanelOpen) return;
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isPanelOpen = true;
            if (panel != null)
            panel.RegisterOnCloseComplete(OnPanelClosed);
            panel.ShowPanel();
        }
    }

    private void OnPanelClosed()
    {
        _isPanelOpen = false;
    }
}