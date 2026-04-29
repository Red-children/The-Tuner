using UnityEngine;
using UnityEngine.UI;
// public enum Canvas
// {
//     Main = 0,
//     System = 1,
// }
public class CanvasManager
{
    private static CanvasManager instance;
    private Canvas _canvasMain;
    private Canvas _canvasSystem;
    public static CanvasManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CanvasManager();
            }
            return instance;
        }
    }

    /// <summary>
    /// 根据指定模式获取或创建对应的画布
    /// </summary>
    /// <param name="mode">画布模式：0 = Main_Canvas，1 = System_Canvas</param>
    /// <returns>
    public Canvas TouchCanvas(int mode)
    {
        //  TODO:Canvas存在时返回Canvas引用，不存在时创建Canvas并返回引用
        return mode switch
        {
            0 => TouchMainCanvas(),
            1 => TouchSystemCanvas(),
            _ => null
        };
    }
    private Canvas TouchMainCanvas()
    {
        if (_canvasMain != null) return _canvasMain;

        var systemObj = GameObject.Find("Canvas_Main");
        if (systemObj != null)
        {
            _canvasSystem = systemObj.GetComponent<Canvas>();
        }

        // 找不到 → 自动创建
        if (_canvasMain == null)
        {
            GameObject canvasObj = new GameObject("Canvas_Main");
            _canvasMain = canvasObj.AddComponent<Canvas>();
            _canvasMain.renderMode = RenderMode.ScreenSpaceOverlay;

            // 必加组件
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
        

        return _canvasMain;
    }
    private Canvas TouchSystemCanvas()
    {
        if (_canvasSystem != null) return _canvasSystem;

        // 按名字查找
        var systemObj = GameObject.Find("Canvas_System");
        if (systemObj != null)
        {
            _canvasSystem = systemObj.GetComponent<Canvas>();
        }

        // 找不到 → 创建
        if (_canvasSystem == null)
        {
            GameObject canvasObj = new GameObject("Canvas_System");
            _canvasSystem = canvasObj.AddComponent<Canvas>();
            _canvasSystem.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvasSystem.sortingOrder = 100; // 系统UI永远在最上层

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        return _canvasSystem;
    }
}
