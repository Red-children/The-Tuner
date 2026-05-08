using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform player; // 把玩家拖进来

    void LateUpdate()
    {
        if (player == null) return;
        Vector3 pos = player.position;
        pos.z = transform.position.z; // 保持摄像机的高度（Z轴不跟角色的前后位置混在一起）
        transform.position = pos;
    }
}