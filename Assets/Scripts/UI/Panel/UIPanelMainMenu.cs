using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

public class UIPanelMainMenu : UIBasePanel
{
    //  主菜单按钮
    [SerializeField] private Button _btnNewGame;
    [SerializeField] private Button _btnSettings;
    [SerializeField] private Button _btnExit;

    void Init()
    {
        //  绑定按钮
        if (_btnNewGame == null || _btnSettings == null || _btnExit == null)
        {
            _btnNewGame = GameObject.Find("ButtonNewGame").GetComponent<Button>();
            _btnSettings = GameObject.Find("ButtonSettings").GetComponent<Button>();
            _btnExit = GameObject.Find("ButtonExit").GetComponent<Button>();
        }
        if (_btnNewGame == null || _btnSettings == null || _btnExit == null)
        {
            Debug.LogError("UIPanelMainmenu 未找到组件");
            return;
        }
        //  TODO:绑定Timeline Director
        if(playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }
        if(playableDirector == null)
        {
            Debug.LogError("UIPanelMainmenu 未找到组件");
        }

        //  绑定按钮事件
        _btnNewGame.onClick.AddListener(OnNewGameClick);
        _btnSettings.onClick.AddListener(OnSettingsClick);
        _btnExit.onClick.AddListener(OnExitClick);
    }
#region 按钮回调函数
    void OnNewGameClick()
    {
        
    }
    void OnSettingsClick()
    {
        
    }
    void OnExitClick()
    {
        UIManager.Instance.ClosePanel(UIManager.UIConst.MainMenu);
    }
#endregion
#region 生命周期
    void Awake()
    {
        Init();
    }
#endregion
}
