using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UISimplePointer : MonoBehaviour
{
    [Header("指向目标")]
    public Transform transTarget;

    [Header("基准点")]
    public Transform transBase;

    [Header("箭头设置")]
    public float screenMargin = 15f;          // 箭头距离屏幕边缘的距离（可调整）
    public float targetOffset = 10f;          // 箭头距离目标的距离
    public float arrowSafeDistance = 20f;     // 箭头自身大小带来的边缘安全距离（新增）

    private RectTransform _rect;
    private Camera _mainCam;
    private Canvas _canvas;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        // _canvas = CanvasManager.Instance.TouchCanvas(0);
        _mainCam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
    }

    void Update()
    {
        if (!transTarget || !transBase)
        {
            gameObject.SetActive(false);
            return;
        }
        UpdateArrow();
    }

    void UpdateArrow()
    {
        gameObject.SetActive(true);

        Vector3 baseScreen = WorldToUIScreen(transBase.position);
        Vector3 targetScreen = WorldToUIScreen(transTarget.position);

        if (targetScreen.z < 0)
        {
            Vector3 dir = (baseScreen - targetScreen).normalized;
            targetScreen = baseScreen + dir * 1000f;
        }

        Vector2 dirVec = (targetScreen - baseScreen).normalized;
        float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
        _rect.rotation = Quaternion.Euler(0, 0, angle);

        Vector2 finalPos;
        if (IsInScreen(targetScreen))
        {
            finalPos = (Vector2)targetScreen - dirVec * targetOffset;
        }
        else
        {
            finalPos = GetScreenEdgePos(baseScreen, dirVec);
        }

        // ✅ 核心新增：让箭头永远不会超出屏幕，保持边缘距离
        finalPos = ClampPositionToScreen(finalPos);

        _rect.position = finalPos;
    }

    // 世界坐标转UI屏幕坐标
    private Vector3 WorldToUIScreen(Vector3 worldPos)
    {
        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return Camera.main.WorldToScreenPoint(worldPos);
        }
        else
        {
            return _mainCam.WorldToScreenPoint(worldPos);
        }
    }

    // 计算屏幕边缘交点
    Vector2 GetScreenEdgePos(Vector2 origin, Vector2 dir)
    {
        float xMin = screenMargin;
        float xMax = Screen.width - screenMargin;
        float yMin = screenMargin;
        float yMax = Screen.height - screenMargin;

        float t = float.MaxValue;
        if (dir.x > 0.001f) t = Mathf.Min(t, (xMax - origin.x) / dir.x);
        if (dir.x < -0.001f) t = Mathf.Min(t, (xMin - origin.x) / dir.x);
        if (dir.y > 0.001f) t = Mathf.Min(t, (yMax - origin.y) / dir.y);
        if (dir.y < -0.001f) t = Mathf.Min(t, (yMin - origin.y) / dir.y);

        return origin + dir * t;
    }

    // ✅ 新增：强制把箭头位置限制在屏幕内，保持安全边距
    Vector2 ClampPositionToScreen(Vector2 pos)
    {
        float left = arrowSafeDistance;
        float right = Screen.width - arrowSafeDistance;
        float bottom = arrowSafeDistance;
        float top = Screen.height - arrowSafeDistance;

        pos.x = Mathf.Clamp(pos.x, left, right);
        pos.y = Mathf.Clamp(pos.y, bottom, top);

        return pos;
    }

    // 判断是否在屏幕内
    bool IsInScreen(Vector2 screenPos)
    {
        return screenPos.x > screenMargin && screenPos.x < Screen.width - screenMargin &&
               screenPos.y > screenMargin && screenPos.y < Screen.height - screenMargin;
    }

    // 动态设置目标
    public void SetTarget(Transform target) => transTarget = target;
}