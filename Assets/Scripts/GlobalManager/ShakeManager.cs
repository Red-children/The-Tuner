using UnityEngine;
using Cinemachine;

public class ShakeManager : MonoBehaviour
{
    public CinemachineImpulseSource impulseSource; // �� Inspector ����ק��ֵ

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
        Debug.Log("启动振动");
        //生成振动+
        impulseSource.GenerateImpulse(new Vector3(e.intensity, e.intensity, 0));
    }
}