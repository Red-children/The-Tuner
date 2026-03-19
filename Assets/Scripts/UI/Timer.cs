using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//  用于延迟执行某些方法
public static class Timer
{
    public static void ResetTimer(this MonoBehaviour mono, string name, float delay)
    {
        mono.CancelInvoke(name);
        mono.Invoke(name, delay);
    }
    public static void StopTimer(this MonoBehaviour mono, string name)
    {
        mono.CancelInvoke(name);
    }
    public static void StartTimer(this MonoBehaviour mono, string name, float delay)
    {
        mono.Invoke(name, delay);
    }
}
