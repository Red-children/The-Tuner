using UnityEngine;

//  进入游戏加载主界面
public class GameEntry : MonoBehaviour
{
    [SerializeField] private SettingData settingData;
    void Awake()
    {
        SettingsManager.Instance.InitSettings(settingData);
    }
    void Start()
    {
        // UIManager.Instance.OpenPanel(UIManager.UIConst.MainMenu);
    }
}
