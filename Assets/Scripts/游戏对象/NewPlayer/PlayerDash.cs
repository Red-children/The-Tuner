using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineImpulseManager.ImpulseEvent;

public class PlayerDash : MonoBehaviour
{
    private PlayerStats stats;
    [Header("闪避设置")]
    public float dashDistance = 3f;          // 最大闪避距离
    public float dashDuration = 0.3f;        // 闪避持续时间

    public bool isDashing = false;             // 是否正在闪避
    public AnimationCurve dashCurve;              // 闪避位移曲线（可选，用于控制闪避的加速/减速效果）

    public float maxDashEnergy = 2;          // 闪避条上限
    public float currentDashEnergy = 2;   // 当前闪避条
    public float dashEnergyRegenRate = 1f;    // 闪避条恢复速率（每秒恢复多少）
    public bool isDashOnWindow = false;             // 是否在节奏窗口内可以闪避

    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        #region  节奏闪避
        if (currentDashEnergy < maxDashEnergy)
        {
            currentDashEnergy += dashEnergyRegenRate * Time.deltaTime;
            if (currentDashEnergy > maxDashEnergy)
                currentDashEnergy = maxDashEnergy;
        }

        // 2. 决定闪避方向
        Vector2 dashDir;

        dashDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - this.transform.position;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");


        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f)
        {
            // 有移动输入：使用WASD方向（归一化）
            dashDir = new Vector2(moveX, moveY).normalized;
        }
        // 3. 计算闪避目标点（考虑墙壁）
        Vector3 targetPos = transform.position + (Vector3)dashDir * dashDistance;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDir, dashDistance, LayerMask.NameToLayer("Wall") );
        if (hit.collider != null)
        {
            Vector2 adjustedPoint = hit.point - dashDir * 0.2f;
            targetPos = new Vector3(adjustedPoint.x, adjustedPoint.y, transform.position.z);
        }

        // 4. 触发闪避
        if (Input.GetMouseButtonDown(1) && (currentDashEnergy > 1 || isDashOnWindow))
        {
            if (!isDashOnWindow)
            {
                currentDashEnergy -= 1;
            }
            StartCoroutine(DashCoroutine(transform.position, targetPos, dashDuration));
        }
        #endregion
    }
    private IEnumerator DashCoroutine(Vector3 start, Vector3 target, float duration)
    {
        isDashing = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveT = dashCurve.Evaluate(t);
            transform.position = Vector3.Lerp(start, target, curveT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target; // 确保最终位置准确

        isDashing = false;
    }
}
