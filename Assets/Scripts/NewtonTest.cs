using System.Collections.Generic;
using UnityEngine;

public static class NewtonInterpolation
{
    /// <summary>
    /// 计算牛顿插值的系数（即各阶差商），并返回一个委托函数，可以反复求值。
    /// </summary>
    /// <param name="xValues">已知节点的 t 坐标（或 x 坐标）</param>
    /// <param name="yValues">已知节点的函数值</param>
    /// <returns>一个接受 t 并返回插值结果的函数</returns>
    public static System.Func<float, float> CreateFunction(float[] xValues, float[] yValues)
    {
        int n = xValues.Length;
        float[] x = new float[n];
        float[] y = new float[n];
        System.Array.Copy(xValues, x, n);
        System.Array.Copy(yValues, y, n);

        // 1. 计算差商表，结果存入 y 数组 (原地保存，节省内存)
        for (int j = 1; j < n; j++)
        {
            for (int i = n - 1; i >= j; i--)
            {
                // 差商核心公式：f[x_i, ..., x_{i-j}] = (f[x_i, ..., x_{i-j+1}] - f[x_{i-1}, ..., x_{i-j}]) / (x_i - x_{i-j})
                y[i] = (y[i] - y[i - 1]) / (x[i] - x[i - j]);
            }
        }

        // 2. 返回一个函数，该函数利用差商表计算任意 t 的插值结果
        return (float t) =>
        {
            float result = y[n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                result = result * (t - x[i]) + y[i];
            }
            return result;
        };
    }

    /// <summary>
    /// 对一组二维点进行插值，返回一个能生成平滑路径的函数。
    /// </summary>
    /// <param name="points">已知的关键点 (巡逻点)</param>
    /// <returns>一个接受 t (0 到 points.Count-1) 并返回插值位置的函数</returns>
    public static System.Func<float, Vector2> CreatePathFunction(Vector2[] points)
    {
        int n = points.Length;
        float[] tValues = new float[n];
        float[] xValues = new float[n];
        float[] yValues = new float[n];

        for (int i = 0; i < n; i++)
        {
            tValues[i] = i;          // 参数 t 就是点的序号
            xValues[i] = points[i].x;
            yValues[i] = points[i].y;
        }

        //对t,x求插值函数
        var xFunc = CreateFunction(tValues, xValues);

        //对t,y求插值函数
        var yFunc = CreateFunction(tValues, yValues);

        return (float t) => new Vector2(xFunc(t), yFunc(t));
    }
}