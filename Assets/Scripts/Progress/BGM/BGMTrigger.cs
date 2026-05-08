using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMTrigger : MonoBehaviour
{
    [SerializeField] private BGMData BGM;
    [SerializeField] private BGMData nextBGM;
    [SerializeField] private bool auto;
    void Start()
    {
        if (auto)
        {
            OnTriggerEnter2D();
            SceneManager.sceneUnloaded += StopOnUnload;
        }

    }

    void Update()
    {
        
    }
    void OnTriggerEnter2D()
    {
        EventBus.Instance.Trigger<PlayBGMEvent>(new(BGM));
    }
    void StopOnUnload(Scene scene)
    {
        EventBus.Instance.Trigger<BGMChangeEvent>(new(nextBGM, false));
    }
}