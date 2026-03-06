using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngineInternal;

public class TempoTrack : MonoBehaviour
{
    int bpm = 120; // 每分钟节拍数
    float secondsPerBeat;// 每拍的秒数


    public void Start()
    {
        secondsPerBeat = 60f / bpm; // 计算每拍的秒数
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z; // 使用角色的屏幕深度
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);


    }
}
