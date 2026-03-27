using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffSelectionHandler : MonoBehaviour
{
    private BuffManager playerBuffManager;
    private bool isWaitingForSelection = false;
    private List<BuffData> currentOptions = new List<BuffData>();
   
    private string[] buffNames;  // 用于显示



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

        // 准备显示用的名称列表
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

    void OnGUI()
    {
        if (!isWaitingForSelection) return;

        // 在屏幕中央显示三个按钮
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
        // 添加Buff
        playerBuffManager.AddBuff(selected);
        Debug.Log($"选择了 {selected.buffName}");

        isWaitingForSelection = false;
        // 触发选择完成事件，让波次继续
        // EventBus.Instance.Trigger(new BuffSelectionCompletedEvent());
    }
}

