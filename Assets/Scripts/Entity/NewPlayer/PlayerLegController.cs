using UnityEngine;

public class PlayerLegController : MonoBehaviour
{
    [Header("组件引用")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        HandleMovementInputAndFacing();
    }

    private void HandleMovementInputAndFacing()
    {
        // 移动输入（WASD/摇杆）
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // 根据鼠标在人物左/右侧决定腿部朝向
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        spriteRenderer.flipX = mouseWorldPos.x > transform.position.x;

        if (playerMovement != null)
            playerMovement.SetMovementInput(moveX, moveY);
    }

    public void PlayLegAttack()
    {
        if (animator != null)
            animator.SetTrigger("LegAttack");
    }

    public void OnLegAttackAnimationEnd()
    {
        if (animator != null)
            animator.ResetTrigger("LegAttack");
    }
}
