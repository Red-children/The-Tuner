using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
   
    // Update is called once per frame
    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z; // 使用角色的屏幕深度this.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }
}
