using UnityEngine;

public class BGMTrigger : MonoBehaviour
{
    [SerializeField] private BGMData BGM;
    [SerializeField] private bool auto;
    void Start()
    {
        if (auto)
            OnTriggerEnter2D();
    }

    void Update()
    {
        
    }
    void OnTriggerEnter2D()
    {
        EventBus.Instance.Trigger<PlayBGMEvent>(new(BGM));
    }
}