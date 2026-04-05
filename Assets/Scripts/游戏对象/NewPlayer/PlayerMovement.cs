using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStats stats;
    private LayerMask wallLayer;
    private bool isStunned = false; // Ӳֱ��־���������ⲿ����

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
        // 调试：检查脚本是否启用
        if (!enabled)
        {
            Debug.LogWarning($"[{name}] PlayerMovement脚本被禁用！");
            return;
        }
        
        if (isStunned) return; // Ӳֱʱ��ֹ�ƶ�

        // 调试：检查输入
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        
        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f)
        {
            Debug.Log($"[{name}] 检测到移动输入: X={moveX:F2}, Y={moveY:F2}");
        }

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