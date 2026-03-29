using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private static UIManager instance;

    private Dictionary<string, string> pathDict;
    private Dictionary<string, GameObject> prefabDict;
    private Dictionary<string, UIBasePanel> panelDict;
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
    // 🔥 找到 Canvas 作为 UI 父物体
    private void InitUIRoot()
    {
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null)
            _uiRoot = canvas.transform;
        else
            Debug.LogError("场景中没有 Canvas! UI无法显示!");
    }
    private void InitDicts()
    {
        pathDict = new Dictionary<string, string>()
        {
            {UIConst.MainMenu, "PanelMainMenu"}
        };
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, UIBasePanel>();
    }
    private UIManager()
    {
        InitDicts();
        InitUIRoot();
    }

    public class UIConst
    {
        public const string MainMenu = "UIPanelMainmenu";
    }


    public UIBasePanel OpenPanel(string name)
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
        
        //  正式打开界面
        GameObject panelObject = GameObject.Instantiate(panelPrefab, _uiRoot, false);
        panel = panelObject.GetComponent<UIBasePanel>();
        panelDict.Add(name, panel);
        return panel;
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
        panelDict.Remove(name);
        return true;
    }

}
