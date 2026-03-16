using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  准星跟鼠标
public class AimingStar : MonoBehaviour
{
    private Vector2 mousePos;
    private void Update()
    {
        //  获取鼠标位置
        mousePos = Input.mousePosition;
        //  将鼠标位置赋值给准星,线性插值避免准星移动过快
        transform.position = Vector2.Lerp(transform.position, mousePos, 0.1f);
    }
}
