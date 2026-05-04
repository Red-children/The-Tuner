using UnityEngine;

public class PlayerIdleArmController : MonoBehaviour
{
    [Header("组件引用")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void SetFlipX(bool flipX)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = flipX;
    }

    public void PlayArmAttack()
    {
        if (animator != null)
            animator.SetTrigger("ArmAttack");
    }

    public void OnArmAttackAnimationEnd()
    {
        if (animator != null)
            animator.ResetTrigger("ArmAttack");
    }
}
