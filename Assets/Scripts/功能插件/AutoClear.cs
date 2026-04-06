using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自动销毁组件，用于在场景中创建的临时对象，如特效、弹幕等，避免占用过多内存和资源。
/// </summary>
public class AutoClear : MonoBehaviour
{
    public int time = 2;
    void Start()
    {
        Destroy(gameObject, time);
    }
}
