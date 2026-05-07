using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private static UIManager instance;

    private Dictionary<string, string> pathDict;
    private Dictionary<string, GameObject> prefabDict;
    private Dictionary<string, UIBasePanel> panelDict;
    private Dictionary<string, int> canvasModeDict;
    private Transform _uiRoot;
    //  单例
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UIManager();
            }
            return instance;
        }
    }
    private UIManager()
    {
        InitDicts();
    }
    private void InitDicts()
    {
        pathDict = new Dictionary<string, string>()
        {
            {UIConst.MainMenu, "PanelMainMenu"},
            {UIConst.Battle, "PanelinBattle"},
            {UIConst.Crosshair, "UICrosshair"},
            {UIConst.Dialogue, "PanelDialogue"},
            {UIConst.PlayerHurt, "PanelPlayerHurtEffect"},
            {UIConst.Pause, "PanelPause"},
            {UIConst.Echo, "PanelEcho"},
            {UIConst.Settings, "PanelSettings"},
            {UIConst.Loading, "PanelLoading"},
        };

        canvasModeDict = new Dictionary<string, int>
        {
            { UIConst.MainMenu, 1 },   // 菜单 → 系统Canvas
            { UIConst.Battle, 0 },     // 战斗 → 主Canvas
            { UIConst.Crosshair, 0 },  // 准星 → 主Canvas
            { UIConst.Dialogue, 1 },   // 对话 → 系统Canvas
            { UIConst.PlayerHurt, 0},  // 受伤 → 主Canvas
            { UIConst.Pause, 1},       // 暂停 → 系统Canvas
            { UIConst.Echo, 0},        // 对话 → 主Canvas
            { UIConst.Settings, 1},    // 设置 → 系统Canvas
            { UIConst.Loading, 1},     // 加载 → 系统Canvas
        };
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, UIBasePanel>();
    }
    public class UIConst
    {
        public const string MainMenu = "UIPanelMainmenu";
        public const string Battle = "UIPanelinBattle";
        public const string Crosshair = "UICrosshair";
        public const string Dialogue = "UIDialogue";
        public const string PlayerHurt = "UIPanelPlayerHurtEffect";
        public const string Pause = "UIPanelPause";
        public const string Echo = "UIPanelEcho";
        public const string Settings = "UIPanelSettings";
        public const string Loading = "UIPanelLoading";
    }

    public UIBasePanel GetPanel(string name)
    {
        if (panelDict.TryGetValue(name, out var panel))
        {
            return panel;
        }
        return null;
    }
    public UIBasePanel OpenPanel(string name, bool visible)
    {
        //  检查是否已经打开
        UIBasePanel panel = null;
        if (panelDict.TryGetValue(name, out panel))
        {
            Debug.LogError($"{name} 界面已打开");
            return null;
        }

        //  检查路径是否存在
        string path = "";
        if (!pathDict.TryGetValue(name, out path))
        {
            Debug.LogError($"{name} 未找到对应路径");
            return null;
        }

        //  检查是否已缓存
        GameObject panelPrefab = null;
        if (!prefabDict.TryGetValue(name, out panelPrefab))
        {
            string realPath = "Prefab/UI/" + path;
            panelPrefab = Resources.Load<GameObject>(realPath);
            prefabDict.Add(name, panelPrefab);
        }

        //  获取Canvas
        int canvasMode = canvasModeDict[name];
        Canvas targetCanvas = CanvasManager.Instance.TouchCanvas(canvasMode);
        Transform parent = targetCanvas.transform;
        
        //  正式打开界面
        GameObject panelObject = GameObject.Instantiate(panelPrefab, parent, false);
        panel = panelObject.GetComponent<UIBasePanel>();
        panelDict.Add(name, panel);
        panel.OpenPanel(name, visible);
        return panel;
    }
    public UIBasePanel OpenPanel(string name)
    {
        return OpenPanel(name, true);
    }

    public bool ClosePanel(string name)
    {
        UIBasePanel panel = null;
        if (!panelDict.TryGetValue(name, out panel))
        {
            Debug.LogError($"{name} 界面未打开");
            return false;
        }

        panel.ClosePanel();
        // panel.RegisterOnCloseComplete(() =>
        // {
        //     panelDict.Remove(name);
        // });
        return true;
    }
    public bool ClosePanel(UIBasePanel panel)
    {
        if (panel == null) return false;
        string name = null;
        foreach (var pair in panelDict)
        {
            if (pair.Value == panel)
            {
                name = pair.Key;
                break;
            }
        }
        panel.ClosePanel();
        // panel.RegisterOnCloseComplete(() =>
        // {
        //     panelDict.Remove(name);
        // });
        return true;
    }
    public void RemovePanel(UIBasePanel panel)
    {
        if (panel == null) return;

        string keyToRemove = null;
        foreach (var pair in panelDict)
        {
            if (pair.Value == panel)
            {
                keyToRemove = pair.Key;
                break;
            }
        }

        if (!string.IsNullOrEmpty(keyToRemove))
        {
            panelDict.Remove(keyToRemove);
        }
    }
    /// <summary>
    /// 场景切换前强制清理面板（同步销毁GameObject并移除字典引用）
    /// 避免异步回调因场景销毁而丢失，导致字典残留脏引用
    /// </summary>
    public void DestroyPanelBeforeSceneSwitch(string name)
    {
        if (panelDict.TryGetValue(name, out UIBasePanel panel))
        {
            panelDict.Remove(name);
            if (panel != null)
            {
                Object.Destroy(panel.gameObject);
            }
        }
    }

    //  隐藏界面（不销毁，保留在字典中）
    public bool HidePanel(string name)
    {
        if (!panelDict.TryGetValue(name, out UIBasePanel panel))
        {
            Debug.LogError($"{name} 界面未打开，无法隐藏");
            return false;
        }

        if (panel == null) 
        {
            panelDict.Remove(name);
            return false;
        }

        panel.HidePanel();
        return true;
    }

    //  新增：显示已隐藏的界面
    public bool ShowPanel(string name)
    {
        if (!panelDict.TryGetValue(name, out UIBasePanel panel))
        {
            Debug.LogError($"{name} 界面未打开，无法显示");
            return false;
        }

        if (panel == null) 
        {
            panelDict.Remove(name);
            return false;
        }

        panel.ShowPanel();
        return true;
    }
}
