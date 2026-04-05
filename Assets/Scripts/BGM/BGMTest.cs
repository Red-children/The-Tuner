using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 开始播放按钮 仅作为测试
/// </summary>
// 挂载在UI Button上即可
public class BGMTest : MonoBehaviour
{
    [SerializeField] private Button _btn;
    public double delay = 100;
    void Awake()
    {
        // 自动获取按钮组件
        _btn = GetComponent<Button>();
        // 绑定点击事件
        _btn.onClick.AddListener(SendPlayBGMEvent);
    }

    // 核心：点击按钮发送事件
    void SendPlayBGMEvent()
    {
        PlayBGMEvent evt = new PlayBGMEvent { time = AudioSettings.dspTime };
        EventBus.Instance.Trigger(evt);
        Debug.Log("BGMTest Button Triggerred");
    }

}
