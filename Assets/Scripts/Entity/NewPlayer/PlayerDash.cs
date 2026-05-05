using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    private PlayerStats stats;

    [Header("冲刺参数")]
    public float dashDistance = 3f;
    public float dashDuration = 0.3f;

    public bool isDashing = false;
    public AnimationCurve dashCurve;

    public float maxDashEnergy = 2;
    public float currentDashEnergy = 2;
    public float dashEnergyRegenRate = 1f;
    public bool isDashOnWindow = false;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (currentDashEnergy < maxDashEnergy)
        {
            currentDashEnergy += dashEnergyRegenRate * Time.deltaTime;
            if (currentDashEnergy > maxDashEnergy)
                currentDashEnergy = maxDashEnergy;
        }

        HandleDashInput();
    }

    private void HandleDashInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TryDash();
        }
    }

    public bool TryDash()
    {
        if (isDashing) return false;
        if (currentDashEnergy < 1 && !isDashOnWindow) return false;

        Vector2 dashDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f)
        {
            dashDir = new Vector2(moveX, moveY).normalized;
        }

        Vector3 targetPos = transform.position + (Vector3)dashDir * dashDistance;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDir, dashDistance, LayerMask.GetMask("Wall"));
        if (hit.collider != null)
        {
            Vector2 adjustedPoint = hit.point - dashDir * 0.5f;
            targetPos = new Vector3(adjustedPoint.x, adjustedPoint.y, transform.position.z);
        }

        if (!isDashOnWindow)
        {
            currentDashEnergy -= 1;
        }

        StartCoroutine(DashCoroutine(transform.position, targetPos, dashDuration));
        return true;
    }

    private IEnumerator DashCoroutine(Vector3 start, Vector3 target, float duration)
    {
        isDashing = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveT = dashCurve.Evaluate(t);
            Vector3 nextPos = Vector3.Lerp(start, target, curveT);
            
            // 每帧检测墙体碰撞
            RaycastHit2D hit = Physics2D.Linecast(transform.position, nextPos, LayerMask.GetMask("Wall"));
            if (hit.collider != null)
            {
                // 碰到墙体，停止冲刺
                transform.position = hit.point - (Vector2)(nextPos - transform.position).normalized * 0.1f;
                break;
            }
            
            transform.position = nextPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}
