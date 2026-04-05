using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStats stats;
    private LayerMask wallLayer;
    private bool isStunned = false;   //是否被眩晕，眩晕时无法移动
    
    // 射线检测长度
    private float rayLengthX = 0.9f;
    private float rayLengthY = 0.9f;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        wallLayer = LayerMask.GetMask("Wall");
    }

    // ���ⲿ���õ�Ӳֱ���Ʒ���
    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
    }

    private void Update()
    {
        if (isStunned) return; // Ӳֱʱ��ֹ�ƶ�

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        if (moveX != 0)
        {
            RaycastHit2D hitX = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(moveX), rayLengthX, wallLayer);
            if (hitX.collider != null) moveX = 0;
        }
        if (moveY != 0)
        {
            RaycastHit2D hitY = Physics2D.Raycast(transform.position, Vector2.up * Mathf.Sign(moveY), rayLengthY, wallLayer);
            if (hitY.collider != null) moveY = 0;
        }

        float moveSpeed = stats != null ? stats.MoveSpeed : 5f;
        transform.Translate(new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime, Space.World);
    }
}