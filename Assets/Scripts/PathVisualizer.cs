using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PathVisualizer : MonoBehaviour
{
    [Header("巡逻点 (手动拖拽或输入坐标)")]
    public Vector2[] waypoints = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(3, 4),
        new Vector2(6, 1),
        new Vector2(9, 5),
        new Vector2(12, 2)
    };

    [Header("路径平滑度")]
    [Range(0.01f, 5f)]
    public float stepSize = 0.05f;

    private System.Func<float, Vector2> pathFunction;

    void Start()
    {
        // 在游戏开始时生成路径函数
        if (waypoints.Length >= 2)
        {
            pathFunction = NewtonInterpolation.CreatePathFunction(waypoints);
        }
    }

    // 在Scene视图绘制蓝线
    void OnDrawGizmos()
    {
        if (waypoints.Length < 2) return;

        // 重新生成路径函数，因为可能在编辑器里修改了巡逻点
        var func = NewtonInterpolation.CreatePathFunction(waypoints);


        Gizmos.color = Color.blue;
        Vector2 prevPoint = func(0); // 起点
        float maxT = waypoints.Length - 1;  // t永远从0到点数-1
        int totalSteps = Mathf.RoundToInt(maxT / stepSize);

        for (int i = 0; i <= totalSteps; i++)
        {
            float t = i * stepSize;
            Vector3 currentPoint = func(t);
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }

        // 在绘制完曲线后，单独绘制节点位置
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector2 nodeAtT = func(i); // t = i 时，函数返回的位置
            Gizmos.DrawWireSphere(nodeAtT, 0.3f); // 绿色圆圈
        }
    }
}