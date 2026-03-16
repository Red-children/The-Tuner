using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class FollowingCam : MonoBehaviour
{
    public string targetTag = "Player";
    public string boundsObjectName = "Confiner";

    private Transform target;
    private CinemachineConfiner2D confiner2D;
    private CinemachineVirtualCamera vCam;
    private void FramingSettings()
    {
        CinemachineFramingTransposer framingTransposer = vCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        framingTransposer.m_UnlimitedSoftZone = false;
        framingTransposer.m_SoftZoneHeight = 0;
        framingTransposer.m_SoftZoneWidth = 0;
    }

    private void ConfinerSettings()
    {
        confiner2D = gameObject.AddComponent<CinemachineConfiner2D>();
        confiner2D.m_BoundingShape2D = GameObject.Find(boundsObjectName).GetComponent<PolygonCollider2D>();
    }
    private void SearchCam()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        if (vCam == null)
        {
            Debug.LogError("No Camera component found");
            return;
        }
    }

    private void FindTarget()
    {
        target = GameObject.FindGameObjectWithTag(targetTag).transform;
        if (target == null)
        {
            Debug.LogError("No target found with tag: " + targetTag);
        }
    }
    private void CameraInit()
    {
        SearchCam();
        FindTarget();
        vCam.Follow = target;
        FramingSettings();
        ConfinerSettings();
    }
    void Start()
    {
        CameraInit();
    }
}
