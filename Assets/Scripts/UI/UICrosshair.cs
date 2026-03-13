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

        // 新增：记录动画初始状态
        if (animBig != null)
        {
            AnimatorStateInfo stateInfo = animBig.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[UICrosshair] 初始动画状态: clip={stateInfo.shortNameHash}, normalizedTime={stateInfo.normalizedTime}, length={stateInfo.length}, loop={stateInfo.loop}");
        }
    }
    private void OnEnable()
    {
        // 订阅你自己的节奏事件
        EventBus.Instance.Subscribe<BeatPreviewEvent>(OnBeatPreview);
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
        EventBus.Instance.Subscribe<PlayerFireEvent>(OnPlayerFire); // 新增
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<BeatPreviewEvent>(OnBeatPreview);
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
        EventBus.Instance.Unsubscribe<PlayerFireEvent>(OnPlayerFire);
    }

    #region 加载图片 资源
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
    #endregion

    #region 获取动画组件
    // 获取动画组件
    private void GetAnimators()
    {
        if (crosshairSmall != null) animSmall = crosshairSmall.GetComponent<Animator>();
        if (crosshairBig != null) animBig = crosshairBig.GetComponent<Animator>();
    }

    #endregion
    // ==================== 事件回调 ====================

    // 节拍预告：大准星开始缩放动画
    private void OnBeatPreview(BeatPreviewEvent evt)
    {
        if (animBig == null) return;

        double now = AudioSettings.dspTime;
        Debug.Log($"[UICrosshair] 预告触发前: now={now:F8}, 动画normalizedTime={animBig.GetCurrentAnimatorStateInfo(0).normalizedTime}");

        animBig.Play("CrosshairNormal", 0, 0f);

        Debug.Log($"[UICrosshair] 预告触发后: now={now:F8}, 动画normalizedTime={animBig.GetCurrentAnimatorStateInfo(0).normalizedTime}");
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

    private float scaleMin = 1f;   // 最小缩放（例如小准星大小）
    private float scaleMax = 1.5f;   // 最大缩放
    private float currentScale = 1f;

    private void Update()
    {
        // 准星跟随鼠标
        transform.position = Input.mousePosition;

        // 根据节拍进度动态缩放（需 RhythmManager 提供 BeatProgress 属性）
        if (RhythmManager.Instance != null)
        {
            float progress = (float)RhythmManager.Instance.BeatProgress; // 0~1
                                                                         // 设计缩放曲线：例如在节拍前 0.2 秒达到最小，之后恢复
                                                                         // 这里简化：将 BeatProgress 映射到缩放
            currentScale = Mathf.Lerp(scaleMax, scaleMin, progress);
            crosshairBig.rectTransform.localScale = Vector3.one * currentScale;
        }
    }

    private void OnPlayerFire(PlayerFireEvent evt)
    {
        if (animSmall == null) return;
        if (evt.isPerfect) // 根据你的判定规则，可能还需要 Great 也算精准
            animSmall.Play("PreciseHit", 0, 0f);
        else
            animSmall.Play("NormalHit", 0, 0f);
    }
}