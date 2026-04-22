using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMTrigger : MonoBehaviour
{
    [SerializeField]private BGMData BGM;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    void OnTriggerEnter2D()
    {
        EventBus.Instance.Trigger<PlayBGMEvent>(new(BGM));
    }
}
