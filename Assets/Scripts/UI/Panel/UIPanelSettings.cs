using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 设置面板 - 纯视觉DEMO
/// 功能：ESC开关、左侧选项卡、右侧滚动、滑块+输入框双向联动
/// </summary>
public class UIPanelSettings : UIBasePanel
{
    [Header("=== 核心面板 ===")]
    public GameObject panelRoot; // 总面板

    [Header("=== 左侧选项卡 ===")]
    public List<GameObject> tabButtons; // 所有选项卡
    public List<GameObject> tabPages;   // 对应右侧页面

    [Header("=== 音量设置（视觉示例） ===")]
    public Slider masterVolumeSlider;
    public InputField masterVolumeInput;

    private int currentTabIndex = 0;

    void Start()
    {
        // 默认隐藏面板
        panelRoot.SetActive(false);

        // 初始化默认选中第一个选项卡
        SwitchTab(0);

        // 绑定音量交互（纯视觉）
        BindVolumeEvents();
    }

    void Update()
    {
        // ESC 呼出/关闭面板
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }
    }

    /// <summary>
    /// 开关面板显示/隐藏
    /// </summary>
    public void TogglePanel()
    {
        bool isActive = !panelRoot.activeSelf;
        panelRoot.SetActive(isActive);
    }

    /// <summary>
    /// 切换左侧选项卡
    /// </summary>
    public void SwitchTab(int index)
    {
        currentTabIndex = index;

        // 隐藏所有页面
        foreach (var page in tabPages)
            page.SetActive(false);

        // 显示选中页面
        tabPages[index].SetActive(true);
    }

    /// <summary>
    /// 绑定滑块与输入框（纯视觉联动）
    /// </summary>
    void BindVolumeEvents()
    {
        // 滑块拖动 → 更新输入框
        masterVolumeSlider.onValueChanged.AddListener((value) =>
        {
            masterVolumeInput.text = Mathf.Round(value).ToString();
        });

        // 输入框输入 → 更新滑块
        masterVolumeInput.onValueChanged.AddListener((text) =>
        {
            if (float.TryParse(text, out float num))
            {
                num = Mathf.Clamp(num, 0, 100);
                masterVolumeSlider.value = num;
            }
        });
    }
}