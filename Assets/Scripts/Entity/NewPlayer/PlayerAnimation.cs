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
        #region ���׷���߼�
        
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        
        Vector2 directionMouse = mouseWorldPos - transform.position;

        if (directionMouse.x > 0)
            spriteRenderer.flipX = false; 
        else if (directionMouse.x < 0)
            spriteRenderer.flipX = true;  
        #endregion
    }
}
