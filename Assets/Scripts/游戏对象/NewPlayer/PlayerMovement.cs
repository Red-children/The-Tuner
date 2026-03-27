using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStats stats;
    private LayerMask wallLayer;
    private bool isStunned = false; // 硬直标志，后续由外部设置

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        wallLayer = LayerMask.GetMask("Wall");
    }

    // 供外部调用的硬直控制方法
    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
    }

    private void Update()
    {
        if (isStunned) return; // 硬直时禁止移动

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        float rayLengthX = 0.9f;
        float rayLengthY = 0.9f;

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