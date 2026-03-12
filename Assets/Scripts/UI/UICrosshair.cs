using UnityEngine;
using UnityEngine.UI;
using System;

public class UICrosshair : MonoBehaviour
{
    [Header("准星组件")]
    public Image crosshairSmall;       // 内圈小准星（用于命中反馈）
    public Image crosshairBig;         // 外圈大准星（用于节奏提示）
    private Animator animSmall;
    private Animator animBig;

    [Header("伤害倍率")]
    private bool isCritical = false;   // 当前是否为精准窗口

    // 图片路径常量（Resources目录下的相对路径）
    private const string SmallCirclePath = "PicUI/CircleSmall";
    private const string BigCirclePath = "PicUI/CircleBig";

    // ==================== 初始化 ====================

    private void Awake()
    {
        crosshairSmall = transform.Find("CrosshairSmall")?.GetComponent<Image>();
        crosshairBig = transform.Find("CrosshairBig")?.GetComponent<Image>();
        if (crosshairSmall == null || crosshairBig == null)
            Debug.LogError("UICrosshair: 未找到 CrosshairSmall 或 CrosshairBig 子物体！");
    }

    private void Start()
    {
        LoadSprites();
        GetAnimators();
        crosshairSmall.enabled = true;
        crosshairBig.enabled = true;
    }

    private void OnEnable()
    {
        // 订阅你自己的节奏事件
        EventBus.Instance.Subscribe<BeatPreviewEvent>(OnBeatPreview);
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<BeatPreviewEvent>(OnBeatPreview);
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
    }

    // 加载图片资源
    private void LoadSprites()
    {
        if (crosshairSmall != null)
        {
            Sprite small = Resources.Load<Sprite>(SmallCirclePath);
            if (small != null)
                crosshairSmall.sprite = small;
            else
                Debug.LogError($"UICrosshair: 加载小准星图片失败，路径 Resources/{SmallCirclePath}.png");
        }

        if (crosshairBig != null)
        {
            Sprite big = Resources.Load<Sprite>(BigCirclePath);
            if (big != null)
                crosshairBig.sprite = big;
            else
                Debug.LogError($"UICrosshair: 加载大准星图片失败，路径 Resources/{BigCirclePath}.png");
        }
    }

    // 获取动画组件
    private void GetAnimators()
    {
        if (crosshairSmall != null) animSmall = crosshairSmall.GetComponent<Animator>();
        if (crosshairBig != null) animBig = crosshairBig.GetComponent<Animator>();
    }

    // ==================== 事件回调 ====================

    // 节拍预告：大准星开始缩放动画
    private void OnBeatPreview(BeatPreviewEvent evt)
    {
        if (animBig == null) return;
        // 从动画开头播放，假设动画长度等于 previewLead
        animBig.Play("CrosshairNormal", 0, 0f);
        Debug.Log($"[UICrosshair] 收到节拍预告 | 时间差：{evt.timeToBeat:F4}");
    }

    // 节奏数据（用于倍率和窗口状态）
    private void OnRhythmData(RhythmData data)
    {
        // 这里可以根据你的需求决定如何设置 isCritical
        // 例如：Perfect 和 Great 视为精准窗口
        isCritical = data.rank == RhythmRank.Perfect || data.rank == RhythmRank.Great;
        Debug.Log($"[UICrosshair] 节奏数据 | rank={data.rank}, isCritical={isCritical}");
    }

    // 敌人命中时由外部调用（例如在玩家攻击命中时发布事件）
    public void OnHit()
    {
        if (animSmall == null) return;
        if (isCritical)
            animSmall.Play("PreciseHit", 0, 0f);
        else
            animSmall.Play("NormalHit", 0, 0f); // 如果有普通命中动画
    }

    // ==================== 每帧更新 ====================

    private void Update()
    {
        // 准星跟随鼠标
        transform.position = Input.mousePosition;
    }
}