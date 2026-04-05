using System.Collections.Generic;
using UnityEngine;

public class BuffSelectionHandler : MonoBehaviour
{
    private BuffManager buffManager;
    private bool isWaitingForSelection = false;
    private List<BuffData> currentOptions = new List<BuffData>();
    private string[] buffNames;

    private void Start()
    {
        // ͨ����ǩ������������ϵ� BuffManager
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            buffManager = player.GetComponent<BuffManager>();
        else
            Debug.LogError("BuffSelectionHandler: δ�ҵ��������");
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<AllWavesCompletedEvent>(OnAllWavesCompleted);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<AllWavesCompletedEvent>(OnAllWavesCompleted);
    }

    /// <summary>
    /// 处理所有波次完成事件：准备Buff选择界面
    /// </summary>
    private void OnAllWavesCompleted(AllWavesCompletedEvent evt)
    {
        // 缓存从事件中接收到的可选Buff项
        currentOptions = evt.buffOptions;
        
        // 安全检查：如果没有可选项，直接退出
        if (currentOptions == null || currentOptions.Count == 0) return;

        // 创建显示名称数组，用于UI界面展示
        // 数组大小与可选Buff数量一致
        buffNames = new string[currentOptions.Count];
        
        // 遍历所有可选Buff，格式化显示名称
        for (int i = 0; i < currentOptions.Count; i++) 
        { 
            // 格式化显示名称："Buff名称 (+数值)"
            buffNames[i] = currentOptions[i].buffName + " (+" + currentOptions[i].value + ")"; 
        }

        // 标记系统进入等待选择状态
        // 就像商店开门营业，等待顾客选择商品
        isWaitingForSelection = true;
    }

    private void Update()
    {
        if (!isWaitingForSelection) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectBuff(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectBuff(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectBuff(2);
    }

    private void OnGUI()
    {
        if (!isWaitingForSelection) return;

        int btnWidth = 200;
        int btnHeight = 50;
        int startX = Screen.width / 2 - btnWidth / 2;
        int startY = Screen.height / 2 - btnHeight * 3 / 2;

        for (int i = 0; i < currentOptions.Count; i++)
        {
            if (GUI.Button(new Rect(startX, startY + i * (btnHeight + 10), btnWidth, btnHeight), buffNames[i]))
            {
                SelectBuff(i);
            }
        }
    }

    private void SelectBuff(int index)
    {
        if (!isWaitingForSelection) return;
        BuffData selected = currentOptions[index];
        buffManager?.AddBuff(selected);
        Debug.Log($"ѡ���� {selected.buffName}");
        isWaitingForSelection = false;
        // �����Ҫ�����Դ���ѡ������¼����ò��μ���
        // EventBus.Instance.Trigger(new BuffSelectionCompletedEvent());
    }
}