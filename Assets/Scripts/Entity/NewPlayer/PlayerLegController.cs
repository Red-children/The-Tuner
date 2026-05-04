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
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f;

        if (moveX > 0.1f)
            spriteRenderer.flipX = true;
        else if (moveX < -0.1f)
            spriteRenderer.flipX = false;

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
