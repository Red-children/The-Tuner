using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

//  按下P键播放BGM并发布计时器上线事件（测试用）
public class BGM : MonoBehaviour
{
    struct TimerOnlineEvent
    {
        public float time; // 事件发生时间
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 播放BGM（示例）
            GetComponent<AudioSource>().Play();
            Debug.Log("播放BGM...");
            // 发布计时器上线事件
            EventBus.Instance.Trigger(new TimerOnlineEvent
            {
                time = Time.time
            });
            Debug.Log($"发布了TimerOnlineEvent(时间:{Time.time:F1}秒)");
        }
        
    }
    
}
