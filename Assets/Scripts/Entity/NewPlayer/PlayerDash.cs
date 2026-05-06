using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        // 改进的终点检测：使用更精确的碰撞检测
        Vector3 targetPos = GetSafeDashTarget(transform.position, dashDir, dashDistance);

        if (!isDashOnWindow)
        {
            currentDashEnergy -= 1;
        }

        StartCoroutine(DashCoroutine(transform.position, targetPos, dashDuration));
        return true;
    }

    private Vector3 GetSafeDashTarget(Vector3 startPos, Vector2 direction, float maxDistance)
    {
        // 使用CircleCast检测玩家大小的路径
        float playerRadius = GetPlayerColliderRadius();
        
        // 检测从起点到最大距离的路径
        RaycastHit2D hit = Physics2D.CircleCast(startPos, playerRadius * 0.8f, direction, 
            maxDistance, LayerMask.GetMask("Wall"));
        
        if (hit.collider != null)
        {
            // 找到碰撞点，在碰撞点前留出安全距离
            float safeDistance = playerRadius + 0.2f;
            float stopDistance = Mathf.Max(0, hit.distance - safeDistance);
            
            // 确保不会停在太近的位置
            if (stopDistance < playerRadius * 2f)
            {
                // 如果安全距离太短，直接不冲刺
                return startPos;
            }
            
            Vector3 safeTarget = startPos + (Vector3)direction * stopDistance;
            Debug.Log($"冲刺安全终点: 原距离{maxDistance}, 安全距离{stopDistance}");
            return safeTarget;
        }
        
        // 没有碰撞，使用最大距离
        return startPos + (Vector3)direction * maxDistance;
    }

    private IEnumerator DashCoroutine(Vector3 start, Vector3 target, float duration)
    {
        // 如果目标位置就是起点，不执行冲刺
        if (Vector3.Distance(start, target) < 0.1f)
        {
            yield break;
        }

        isDashing = true;

        float elapsed = 0f;
        Vector3 lastPosition = start;
        
        // 获取玩家碰撞体信息用于检测
        float playerRadius = GetPlayerColliderRadius();
        
        // Tilemap墙体层级
        LayerMask wallLayer = LayerMask.GetMask("Wall");

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveT = dashCurve.Evaluate(t);
            Vector3 nextPos = Vector3.Lerp(start, target, curveT);
            
            // 实时检测Tilemap碰撞
            if (DetectTilemapCollision(lastPosition, nextPos, playerRadius, wallLayer))
            {
                // 碰到Tilemap墙体，停止冲刺
                Debug.Log($"冲刺碰到Tilemap墙体，停止在: {lastPosition}");
                break;
            }
            
            transform.position = nextPos;
            lastPosition = nextPos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终位置准确
        transform.position = lastPosition;

        isDashing = false;
    }
    
    /// <summary>
    /// 检测Tilemap碰撞
    /// </summary>
    private bool DetectTilemapCollision(Vector3 from, Vector3 to, float radius, LayerMask wallLayer)
    {
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        
        // 使用CircleCast检测玩家大小的路径
        RaycastHit2D hit = Physics2D.CircleCast(from, radius, direction, distance + 0.1f, wallLayer);
        
        if (hit.collider != null)
        {
            // 特别检查是否是Tilemap碰撞体
            if (hit.collider.GetComponent<TilemapCollider2D>() != null || 
                hit.collider.gameObject.name.Contains("Tilemap") ||
                hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                Debug.Log($"检测到Tilemap碰撞: {hit.collider.name}, 距离: {hit.distance}");
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取玩家碰撞体半径
    /// </summary>
    private float GetPlayerColliderRadius()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider is CircleCollider2D circleCollider)
        {
            return circleCollider.radius;
        }
        else if (collider is CapsuleCollider2D capsuleCollider)
        {
            return Mathf.Max(capsuleCollider.size.x, capsuleCollider.size.y) * 0.5f;
        }
        else if (collider is BoxCollider2D boxCollider)
        {
            return Mathf.Max(boxCollider.size.x, boxCollider.size.y) * 0.5f;
        }
        
        // 默认半径
        return 0.5f;
    }
}
