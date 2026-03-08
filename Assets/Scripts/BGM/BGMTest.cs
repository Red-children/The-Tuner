using UnityEngine;
using UnityEngine.UI;

// 挂载在UI Button上即可
public class BGMTest : MonoBehaviour
{
    private Button _btn;
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
        // 1. 构造播放BGM事件（用你GameEvents里的结构体）
        PlayBGMEvent evt = new PlayBGMEvent
        {
            time = AudioSettings.dspTime + delay // 事件发生时间（可选）
        };

        // 2. 发送到事件中心
        PreciseEventBus.Instance.Trigger(evt);
        Debug.Log("BGMTest测试按钮:已发送播放BGM事件!");
    }
}
