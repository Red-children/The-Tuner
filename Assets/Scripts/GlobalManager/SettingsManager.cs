using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsManager
{
    private static SettingsManager instance;
    private static readonly object _lock = new object();
    
    public static SettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                        instance = new SettingsManager();
                }
            }
            return instance;
        }
    }

    private Dictionary<SettingType, SettingsEntry> _entryDict;
    private readonly object _dictLock = new object();
    //  数据修改回调
    private Dictionary<SettingType, Action> _callbacks;
#region 数据操作
    // 初始化（只调用一次）
    public bool InitSettings(SettingData data)
    {
        lock (_dictLock)
        {
            if (_entryDict != null)
            {
                Debug.LogWarning("已初始化，忽略重复调用");
                return false;
            }

            if (data == null) return false;

            _entryDict = new Dictionary<SettingType, SettingsEntry>();
            _callbacks = new Dictionary<SettingType, Action>();
            var entries = data.Init();
            
            foreach (var entry in entries)
            {
                _entryDict[entry.settingType] = entry;
                _callbacks[entry.settingType] = null;
            }
            return true;
        }
    }

    // 取值
    public int GetValue(SettingType type)
    {
        lock (_dictLock)
        {
            if (_entryDict == null) return -1;
            return _entryDict.TryGetValue(type, out var e) ? e.amount : -1;
        }
    }
    //  取所有设置
    public Dictionary<SettingType, SettingsEntry> GetValues()
    {
        lock(_dictLock)
        {
            if (_entryDict == null) return null;
            
            var copy = new Dictionary<SettingType, SettingsEntry>();
            foreach (var kvp in _entryDict)
            {
                copy[kvp.Key] = new SettingsEntry
                {
                    settingType = kvp.Value.settingType,
                    name = kvp.Value.name,
                    amount = kvp.Value.amount,
                    available = kvp.Value.available
                };
            }
            return copy;
        }
    }
    // 改值
    public bool SetValue(SettingType type, int value)
    {
        lock (_dictLock)
        {
            if (_entryDict == null) return false;
            if (!_entryDict.TryGetValue(type, out var e))
                return false;
            
            int newVal = Mathf.Clamp(value, 0, 100);
            if (e.amount == newVal) return true;

            e.amount = newVal;
            TriggerCallback(type);
            return true;
        }
    }

    // 刷新
    public void RefreshSettings(SettingData data)
    {
        lock (_dictLock)
        {
            if (_entryDict == null || data == null) return;
            
            _entryDict.Clear();
            var entries = data.Init();
            foreach (var entry in entries)
            {
                _entryDict[entry.settingType] = entry;
            }
        }
    }
#endregion

#region 回调相关
    public void RegisterCallback(SettingType type, Action func)
    {
        if (!_callbacks.ContainsKey(type))
        {
            Debug.LogError("Setting Callback Register Failed:No Such Type of Settings\n");
            return;
        }
        _callbacks[type] += func;
    }
    public void UnregisterCallback(SettingType type, Action func)
    {
        if (!_callbacks.ContainsKey(type))
        {
            Debug.LogError("Setting Callback Unregister Failed:No Such Type of Settings\n");
            return;
        }
        _callbacks[type] -= func;
    }
    private void TriggerCallback(SettingType type)
    {
        if (!_callbacks.ContainsKey(type))
        {
            Debug.LogError("Setting Callback Trigger Failed:No Such Type of Settings\n");
            return;
        }
        _callbacks[type]?.Invoke();
    }

#endregion
}