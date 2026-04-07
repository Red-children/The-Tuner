using UnityEngine;

public class BossInfo : MonoBehaviour
{
    public Animator animator;

    public void PlayIdle()
    {
        if (animator != null)
            animator.Play("Idle");
    }

    public void PlayAttack()
    {
        if (animator != null)
            animator.Play("Attack");
    }

    public void PlayHurt()
    {
        if (animator != null)
            animator.Play("Hurt");
    }

    public void PlayDeath()
    {
        if (animator != null)
            animator.Play("Death");
    }
}