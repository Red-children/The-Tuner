using UnityEngine;
using System.Collections;

public class OffKeyBugDash : BaseEnemy
{
    [Header("≥Â¥Ã≤Œ ˝")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;
    public float cooldown = 1.5f;
    public float damage = 10f;
    public float hitRadius = 1.5f;

    private bool isDashing = false;
    private float cooldownTimer = 0f;
    private Vector3 startPos;
    private float dashTimer;

    private void Update()
    {
        if (target == null) return;

        // ¿‰»¥÷–
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // ≥Â¥Ã÷–
        if (isDashing)
        {
            dashTimer += Time.deltaTime;
            float t = dashTimer / dashDuration;
            transform.position = Vector3.Lerp(startPos, target.position, t);
            if (t >= 1f)
            {
                // ≥Â¥ÃΩ· ¯£¨…À∫¶≈–∂®
                if (Vector2.Distance(transform.position, target.position) < hitRadius)
                {
                    PlayerAPI player = target.GetComponent<PlayerAPI>();
                    player?.TakeDamage((int)damage);
                }
                isDashing = false;
                cooldownTimer = cooldown;
            }
            return;
        }

        // ºÏ≤‚Ω⁄≈ƒ
        if (RhythmManager.Instance != null)
        {
            float progress = (float)RhythmManager.Instance.BeatProgress;
            if (progress > 0.95f)
            {
                startPos = transform.position;
                dashTimer = 0f;
                isDashing = true;
            }
        }
    }
}
