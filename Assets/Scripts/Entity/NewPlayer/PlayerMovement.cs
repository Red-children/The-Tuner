using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStats stats;
    private LayerMask wallLayer;
    private bool isStunned = false;

    private float rayLengthX = 0.9f;
    private float rayLengthY = 0.9f;

    private float currentMoveX = 0f;
    private float currentMoveY = 0f;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        wallLayer = LayerMask.GetMask("Wall");
    }

    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
    }

    public void SetMovementInput(float moveX, float moveY)
    {
        currentMoveX = moveX;
        currentMoveY = moveY;
    }

    private void Update()
    {
        if (isStunned) return;

        float moveX = currentMoveX;
        float moveY = currentMoveY;

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