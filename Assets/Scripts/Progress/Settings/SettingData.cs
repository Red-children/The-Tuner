using System.Collections.Generic;
using UnityEngine;

//  设置注册表
public enum SettingType
{
    MasterVolume = 0,
    SFXVolume = 1,
}

[System.Serializable]
public struct SettingsEntry
{
    public SettingType settingType;
    public string name;
    [Range(0, 100)]
    public int amount;
    public bool available;
}
/// <summary>
/// 数据层,初始化时将数据传入Manager
/// </summary>
[CreateAssetMenu(fileName = "NewSettingData", menuName = "Setting/SettingData")]
public class SettingData : ScriptableObject
{
    [SerializeField] private List<SettingsEntry> entries;
    private List<SettingsEntry> Init()
    {
        return new List<SettingsEntry>(entries);
    }

    private void UpdataSettings(SettingType type, int val)
    {
        for(int i = 0; i < entries.Count; ++i)
        {
            if  (entries[i].settingType != type) continue;
            var entry = entries[i];
            entry.amount = Mathf.Clamp(val, 0, 100);
            entries[i] = entry;
            break;
        }
    }
}
