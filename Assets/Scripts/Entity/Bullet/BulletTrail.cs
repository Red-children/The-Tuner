using System.Collections.Generic;
using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [Header("拖尾配置")]
    public LineRenderer lineRenderer;
    public float trailDuration = 0.3f;
    public int maxPositionCount = 20;
    public float startWidth = 0.5f;
    public float endWidth = 0f;
    public Gradient trailGradient;

    private List<Vector3> positions = new List<Vector3>();

    private void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;
            lineRenderer.colorGradient = trailGradient;
            lineRenderer.sortingOrder = GetComponent<SpriteRenderer>()?.sortingOrder - 1 ?? 0;
        }
    }

    private void Update()
    {
        AddPosition(transform.position);
        UpdateTrail();
    }

    private void AddPosition(Vector3 newPos)
    {
        positions.Add(newPos);
        if (positions.Count > maxPositionCount)
        {
            positions.RemoveAt(0);
        }
    }

    private void UpdateTrail()
    {
        if (positions.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        float alphaPerNode = 1f / positions.Count;
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[positions.Count];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[positions.Count];

        for (int i = 0; i < positions.Count; i++)
        {
            colorKeys[i].time = (float)i / (positions.Count - 1);
            colorKeys[i].color = Color.white;
            alphaKeys[i].time = (float)i / (positions.Count - 1);
            alphaKeys[i].alpha = (float)(i + 1) / positions.Count;
        }

        gradient.colorKeys = colorKeys;
        gradient.alphaKeys = alphaKeys;
        lineRenderer.colorGradient = gradient;
    }

    private void OnDisable()
    {
        positions.Clear();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }
}
