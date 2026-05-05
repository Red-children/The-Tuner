using UnityEngine;

public struct InitComplete
{
    bool success;
    public float t;
    public InitComplete(bool isInitialized, float time)
    {
        success = isInitialized;
        t = time;
    }
}
//  进入游戏加载主界面
public class GameEntry : MonoBehaviour
{
    [SerializeField] private SettingData settingData;
    private bool _isInitialized = false;
    void Awake()
    {
        _isInitialized = SettingsManager.Instance.InitSettings(settingData);
    }
    void Start()
    {
        if (_isInitialized)
        {
            EventBus.Instance.Trigger(new InitComplete(_isInitialized, Time.time));
            Debug.Log("Init Complete");
        }

        UIManager.Instance.OpenPanel(UIManager.UIConst.MainMenu);
    }
}
