using System.Collections.Generic;
using UnityEngine;

//  设置注册表
public enum SettingType
{
    MasterVolume = 0,
    SFXVolume = 1,
}

[System.Serializable]
public class SettingsEntry
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
    public List<SettingsEntry> Init()
    {
        return new List<SettingsEntry>(entries);
    }
    public void UpdateSettings(List<SettingsEntry> newEntries)
    {
        entries.Clear();
        entries.AddRange(newEntries);
    }
}
