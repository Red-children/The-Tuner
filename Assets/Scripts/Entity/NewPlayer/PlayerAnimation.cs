using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();   
    }

    // Update is called once per frame
    void Update()
    {
        #region 鼠标追踪逻辑
        // 获取鼠标在世界空间中的位置（注意：ScreenToWorldPoint 需要正确的Z值）
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z; // 使用角色的屏幕深度
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // 计算向量（从角色指向鼠标）
        Vector2 directionMouse = mouseWorldPos - transform.position;

        if (directionMouse.x > 0)
            spriteRenderer.flipX = false; // 朝右
        else if (directionMouse.x < 0)
            spriteRenderer.flipX = true;  // 朝左       
                                          // 注意：这里仅根据X轴方向决定翻转，如果鼠标在正上方，会保持上次朝向，但通常够用。
                                          // 更精细的可以结合方向角度，但先这样。
        #endregion
    }
}
