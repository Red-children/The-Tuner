using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public static class RhythmInputDebugOverlayBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Object.FindObjectOfType<RhythmInputDebugOverlay>() != null)
        {
            return;
        }

        GameObject go = new GameObject("RhythmInputDebugOverlay");
        go.AddComponent<RhythmInputDebugOverlay>();
    }
}
#endif

public class RhythmInputDebugOverlay : MonoBehaviour
{
    private const float PanelWidth = 760f;
    private const float PanelHeight = 64f;
    private const float TrackWidth = 660f;
    private const float TrackHeight = 18f;
    private const float HistoryMarkerWidth = 2f;
    private const float HistoryMarkerHeight = 20f;
    private const float LatestMarkerWidth = 3f;
    private const float LatestMarkerHeight = 28f;
    private const float RunnerWidth = 4f;
    private const float RunnerHeight = 24f;
    private const float PulseWidth = 10f;
    private const float PulseHeight = 28f;

    private sealed class MarkerView
    {
        public RectTransform rect;
        public Image image;
        public float bornTime;
        public bool isLatest;
    }

    [SerializeField] private KeyCode toggleKey = KeyCode.F8;
    [SerializeField] private float markerLifetime = 2.0f;
    [SerializeField] private int maxMarkers = 24;

    private readonly List<MarkerView> markers = new List<MarkerView>();

    private RectTransform panelRect;
    private RectTransform markerLayerRect;
    private RectTransform runnerRect;
    private RectTransform pulseRect;
    private Text infoText;
    private Text statusText;
    private Text shotText;
    private bool isVisible = true;
    private float pulseStartTime = -10f;
    private float shotTextStartTime = -10f;

    private void Awake()
    {
        if (Object.FindObjectsOfType<RhythmInputDebugOverlay>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        EnsureCanvas();
        BuildView();
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<RhythmInputDebugEvent>(OnRhythmInputDebugEvent);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<RhythmInputDebugEvent>(OnRhythmInputDebugEvent);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetVisible(!isVisible);
        }

        UpdateRunner();
        UpdatePulse();
        UpdateMarkers();
    }

    private void EnsureCanvas()
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = gameObject.AddComponent<RectTransform>();
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster = gameObject.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }

        raycaster.enabled = false;
    }

    private void BuildView()
    {
        panelRect = CreateRect("Panel", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(PanelWidth, PanelHeight));
        Image panelImage = panelRect.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.06f, 0.86f);

        statusText = CreateText("Status", panelRect, string.Empty, 13, TextAnchor.MiddleRight);
        SetAnchoredBox(statusText.rectTransform, new Vector2(1f, 1f), new Vector2(-10f, -8f), new Vector2(160f, 16f));
        statusText.color = new Color(0.72f, 0.72f, 0.72f, 0.95f);

        infoText = CreateText("Info", panelRect, "0.0 ms", 18, TextAnchor.MiddleLeft);
        SetAnchoredBox(infoText.rectTransform, new Vector2(0f, 1f), new Vector2(12f, -10f), new Vector2(220f, 20f));
        infoText.color = new Color(0.96f, 0.96f, 0.96f, 0.98f);

        shotText = CreateText("ShotText", panelRect, string.Empty, 20, TextAnchor.MiddleCenter);
        SetAnchoredBox(shotText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(120f, 16f));
        shotText.color = new Color(1f, 1f, 1f, 0f);

        BuildTrack();
    }

    private void BuildTrack()
    {
        RectTransform trackRect = CreateRect("Track", panelRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 9f), new Vector2(TrackWidth, TrackHeight));
        Image trackImage = trackRect.gameObject.AddComponent<Image>();
        trackImage.color = new Color(0.12f, 0.12f, 0.13f, 1f);

        CreateGuideLine("LeftGuide", trackRect, -TrackWidth * 0.5f, 1f, 4f, 0.18f);
        CreateGuideLine("QuarterLeftGuide", trackRect, -TrackWidth * 0.25f, 1f, 2f, 0.1f);
        CreateGuideLine("CenterGuide", trackRect, 0f, 2f, 14f, 0.95f);
        CreateGuideLine("QuarterRightGuide", trackRect, TrackWidth * 0.25f, 1f, 2f, 0.1f);
        CreateGuideLine("RightGuide", trackRect, TrackWidth * 0.5f, 1f, 4f, 0.18f);

        pulseRect = CreateRect("CenterPulse", trackRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PulseWidth, PulseHeight));
        Image pulseImage = pulseRect.gameObject.AddComponent<Image>();
        pulseImage.color = new Color(1f, 1f, 1f, 0f);

        markerLayerRect = CreateRect("MarkerLayer", trackRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(TrackWidth, TrackHeight));

        runnerRect = CreateRect("Runner", markerLayerRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-TrackWidth * 0.5f, 0f), new Vector2(RunnerWidth, RunnerHeight));
        Image runnerImage = runnerRect.gameObject.AddComponent<Image>();
        runnerImage.color = new Color(1f, 1f, 1f, 0.92f);

    }

    private void UpdateRunner()
    {
        if (runnerRect == null)
        {
            return;
        }

        RhythmManager rhythm = RhythmManager.Instance;
        if (rhythm == null || rhythm.bpm <= 0)
        {
            runnerRect.anchoredPosition = new Vector2(-TrackWidth * 0.5f, 0f);
            statusText.text = string.Empty;
            return;
        }

        double beatInterval = 60.0 / rhythm.bpm;
        if (beatInterval <= 0)
        {
            runnerRect.anchoredPosition = new Vector2(-TrackWidth * 0.5f, 0f);
            statusText.text = string.Empty;
            return;
        }

        RhythmManager.RankResult sample = rhythm.GetDebugRankAtTime(AudioSettings.dspTime);
        double halfInterval = beatInterval * 0.5;
        float normalized = halfInterval <= 0
            ? 0f
            : Mathf.Clamp((float)(sample.offsetSeconds / halfInterval), -1f, 1f);

        float x = normalized * (TrackWidth * 0.5f);
        runnerRect.anchoredPosition = new Vector2(x, 0f);
        statusText.text = string.Empty;
    }

    private void OnRhythmInputDebugEvent(RhythmInputDebugEvent evt)
    {
        if (!isVisible || markerLayerRect == null)
        {
            return;
        }

        MarkPreviousMarkersAsHistory();

        float x = GetTrackPosition(evt);
        TriggerShotFeedback(x);
        RectTransform markerRect = CreateRect("InputMarker", markerLayerRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(LatestMarkerWidth + 3f, LatestMarkerHeight + 14f));
        Image markerImage = markerRect.gameObject.AddComponent<Image>();
        markerImage.color = new Color(1f, 1f, 1f, 1f);

        MarkerView marker = new MarkerView
        {
            rect = markerRect,
            image = markerImage,
            bornTime = Time.unscaledTime,
            isLatest = true
        };
        markers.Add(marker);

        while (markers.Count > maxMarkers)
        {
            DestroyMarker(markers[0]);
            markers.RemoveAt(0);
        }

        infoText.text = BuildOffsetText(evt.offsetMilliseconds);
    }

    private float GetTrackPosition(RhythmInputDebugEvent evt)
    {
        RhythmManager rhythm = RhythmManager.Instance;
        double beatInterval = rhythm != null && rhythm.bpm > 0
            ? 60.0 / rhythm.bpm
            : 0.5;
        double halfInterval = beatInterval * 0.5;

        float normalized = halfInterval <= 0
            ? 0f
            : Mathf.Clamp((float)(evt.offsetSeconds / halfInterval), -1f, 1f);

        return normalized * (TrackWidth * 0.5f);
    }

    private void UpdateMarkers()
    {
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            MarkerView marker = markers[i];
            float life = (Time.unscaledTime - marker.bornTime) / markerLifetime;
            if (life >= 1f)
            {
                DestroyMarker(marker);
                markers.RemoveAt(i);
                continue;
            }

            float alpha = marker.isLatest ? 0.9f - (life * 0.35f) : 0.45f * (1f - life);
            marker.image.color = new Color(1f, 1f, 1f, alpha);
            marker.rect.sizeDelta = marker.isLatest
                ? new Vector2(LatestMarkerWidth + 3f, LatestMarkerHeight + 14f)
                : new Vector2(HistoryMarkerWidth, HistoryMarkerHeight);
        }
    }

    private void UpdatePulse()
    {
        if (pulseRect != null)
        {
            float pulseAge = Time.unscaledTime - pulseStartTime;
            Image pulseImage = pulseRect.GetComponent<Image>();
            if (pulseImage != null)
            {
                if (pulseAge >= 0f && pulseAge <= 0.22f)
                {
                    float t = pulseAge / 0.22f;
                    pulseImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.7f, 0f, t));
                    pulseRect.sizeDelta = new Vector2(Mathf.Lerp(PulseWidth, PulseWidth + 6f, t), Mathf.Lerp(PulseHeight, PulseHeight + 4f, t));
                }
                else
                {
                    pulseImage.color = new Color(1f, 1f, 1f, 0f);
                    pulseRect.sizeDelta = new Vector2(PulseWidth, PulseHeight);
                }
            }
        }

        if (shotText != null)
        {
            float textAge = Time.unscaledTime - shotTextStartTime;
            if (textAge >= 0f && textAge <= 0.32f)
            {
                float t = textAge / 0.32f;
                shotText.text = string.Empty;
                shotText.color = new Color(1f, 1f, 1f, 0f);
                shotText.rectTransform.anchoredPosition = new Vector2(0f, 28f);
            }
            else
            {
                shotText.text = string.Empty;
                shotText.color = new Color(1f, 1f, 1f, 0f);
                shotText.rectTransform.anchoredPosition = new Vector2(0f, 28f);
            }
        }
    }

    private void MarkPreviousMarkersAsHistory()
    {
        for (int i = 0; i < markers.Count; i++)
        {
            markers[i].isLatest = false;
        }
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        if (panelRect != null)
        {
            panelRect.gameObject.SetActive(visible);
        }
    }

    private void TriggerShotFeedback(float x)
    {
        pulseStartTime = Time.unscaledTime;
        shotTextStartTime = Time.unscaledTime;
        if (pulseRect != null)
        {
            pulseRect.anchoredPosition = new Vector2(x, 0f);
        }
        if (shotText != null)
        {
            shotText.text = string.Empty;
        }
    }

    private static string BuildOffsetText(double offsetMilliseconds)
    {
        return offsetMilliseconds >= 0
            ? string.Format("+{0:0.0} ms", offsetMilliseconds)
            : string.Format("{0:0.0} ms", offsetMilliseconds);
    }

    private static void CreateGuideLine(string name, RectTransform parent, float x, float width, float height, float alpha)
    {
        RectTransform lineRect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(width, TrackHeight + height));
        Image lineImage = lineRect.gameObject.AddComponent<Image>();
        lineImage.color = new Color(1f, 1f, 1f, alpha);
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        return text;
    }

    private static Image CreateBand(string name, RectTransform parent, Color color)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, TrackHeight));
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static void SetAnchoredBox(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void DestroyMarker(MarkerView marker)
    {
        if (marker != null && marker.rect != null)
        {
            Object.Destroy(marker.rect.gameObject);
        }
    }
}
