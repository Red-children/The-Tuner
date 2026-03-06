using UnityEngine;
using UnityEngine.UI;

public class UICrosshair : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas parentCanvas;

    void Start()
    {
        // 获取组件
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        // 检查是否在 Canvas 下
        if (parentCanvas == null)
        {
            Debug.LogError("UICrosshair must be placed under a Canvas!");
        }
    }

    void Update()
    {
        // 获取鼠标屏幕坐标
        Vector2 screenPos = Input.mousePosition;

        // 将屏幕坐标转换为 Canvas 内部的局部坐标
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPos,
            parentCanvas.worldCamera,    // 对于 Overlay 模式，传入 null 即可，但保留字段更通用
            out localPos);

        // 设置 UI 元素的本地坐标
        rectTransform.localPosition = localPos;
    }
}
