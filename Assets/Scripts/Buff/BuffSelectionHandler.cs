using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffSelectionHandler : MonoBehaviour
{
    private BuffManager playerBuffManager;
    private bool isWaitingForSelection = false;
    private List<BuffData> currentOptions = new List<BuffData>();

    private void Start()
    {
        PlayerIObject player = FindObjectOfType<PlayerIObject>();
        if (player != null)
            playerBuffManager = player.GetComponent<BuffManager>();
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<AllWavesCompletedEvent>(OnAllWavesCompleted);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<AllWavesCompletedEvent>(OnAllWavesCompleted);
    }

    private void OnAllWavesCompleted(AllWavesCompletedEvent evt)
    {
        currentOptions = evt.buffOptions;
        if (currentOptions == null || currentOptions.Count == 0) return;

        // 显示选项（这里先用 Debug，后续替换为 UI）
        Debug.Log("=== 请选择一项增益 ===");
        for (int i = 0; i < currentOptions.Count; i++)
        {
            Debug.Log($"{i + 1}. {currentOptions[i].buffName} (+{currentOptions[i].value})");
        }

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

    private void SelectBuff(int index)
    {
        if (!isWaitingForSelection) return;
        if (index < 0 || index >= currentOptions.Count) return;

        BuffData selected = currentOptions[index];
        if (playerBuffManager != null)
        {
            playerBuffManager.AddBuff(selected);
            Debug.Log($"选择了 {selected.buffName}");
        }

        isWaitingForSelection = false;
        // 可选：触发一个选择完成事件，但这里不需要通知 WaveManager 了，因为所有波次已完成
    }
}
