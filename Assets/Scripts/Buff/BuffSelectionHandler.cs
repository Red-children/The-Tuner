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
        // 通过标签查找玩家物体上的 BuffManager
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            buffManager = player.GetComponent<BuffManager>();
        else
            Debug.LogError("BuffSelectionHandler: 未找到玩家物体");
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

        buffNames = new string[currentOptions.Count];
        for (int i = 0; i < currentOptions.Count; i++)
        {
            buffNames[i] = currentOptions[i].buffName + " (+" + currentOptions[i].value + ")";
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
        Debug.Log($"选择了 {selected.buffName}");
        isWaitingForSelection = false;
        // 如果需要，可以触发选择完成事件，让波次继续
        // EventBus.Instance.Trigger(new BuffSelectionCompletedEvent());
    }
}