using UnityEngine;

/// <summary>
/// 调试工具：查找场景中所有的 AudioListener 组件
/// </summary>
public class FindAudioListeners : MonoBehaviour
{
    private void Start()
    {
        FindAllAudioListeners();
    }

    [ContextMenu("查找所有 AudioListener")]
    private void FindAllAudioListeners()
    {
        // 查找场景中所有的 AudioListener 组件
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        Debug.Log($"========== 找到 {listeners.Length} 个 AudioListener ==========");
        
        foreach (AudioListener listener in listeners)
        {
            GameObject obj = listener.gameObject;
            Debug.Log($"AudioListener 位置: {obj.name}");
            Debug.Log($"  └─ 层级路径: {GetHierarchyPath(obj.transform)}");
            Debug.Log($"  └─ 是否启用: {listener.enabled}");
            Debug.Log($"  └─ 对象标签: {obj.tag}");
            Debug.Log("");
        }
        
        if (listeners.Length > 1)
        {
            Debug.LogWarning("警告：场景中有多个 AudioListener！这可能导致音频问题！");
            Debug.LogWarning("建议只保留一个 AudioListener（通常在主摄像机上）");
        }
        else if (listeners.Length == 0)
        {
            Debug.LogWarning("警告：场景中没有找到 AudioListener！");
        }
        else
        {
            Debug.Log("✓ 场景中只有一个 AudioListener，配置正确");
        }
    }

    /// <summary>
    /// 获取对象在层级视图中的完整路径
    /// </summary>
    private string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
}