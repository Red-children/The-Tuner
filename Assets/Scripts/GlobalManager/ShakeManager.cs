using UnityEngine;
using Cinemachine;

public class ShakeManager : MonoBehaviour
{
    public CinemachineImpulseSource impulseSource; // 在 Inspector 中拖拽赋值

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<CameraShakeEvent>(OnCameraShake);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<CameraShakeEvent>(OnCameraShake);
    }

    private void OnCameraShake(CameraShakeEvent e)
    {
        if (impulseSource == null) return;
        Debug.Log("振动事件正常接受");
        // 使用事件中的强度生成震动，可以根据需要乘以一个全局系数
        // GenerateImpulse 可以传入一个速度向量，这里简单使用三方向等强度
        impulseSource.GenerateImpulse(new Vector3(e.intensity, e.intensity, 0));
    }
}