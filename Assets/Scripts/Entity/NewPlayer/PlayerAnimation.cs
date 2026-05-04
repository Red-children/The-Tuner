using UnityEngine;

public class PlayerAnimation : MonoBehaviour
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
        HandleFacing();
        HandleMovementInput();
    }

    private void HandleFacing()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Vector2 directionMouse = mouseWorldPos - transform.position;

        if (directionMouse.x > 0)
            spriteRenderer.flipX = true;
        else if (directionMouse.x < 0)
            spriteRenderer.flipX = false;
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f;

       // animator.SetBool("isMoving", isMoving);

        if (playerMovement != null)
            playerMovement.SetMovementInput(moveX, moveY);
    }

    // 动画事件回调方法 - 由动画事件调用
    public void OnShootAnimationEnd()
    {
        animator.SetBool("isShooting", false);
    }

    public void OnMeleeAnimationEnd()
    {
        animator.SetBool("isMeleeing", false);
    }

    public void OnDashAnimationEnd()
    {
        animator.SetBool("isDashing", false);
    }

    public void OnChargeAnimationEnd()
    {
        animator.SetBool("isCharging", false);
    }

    public void PlayBodyAttack()
    {
        if (animator != null)
            animator.SetTrigger("BodyAttack");
    }

    public void OnBodyAttackAnimationEnd()
    {
        if (animator != null)
            animator.ResetTrigger("BodyAttack");
    }
}
