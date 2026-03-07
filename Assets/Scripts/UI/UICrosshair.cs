using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// 准星UI
public class UICrosshair : MonoBehaviour
{

    [Header("准星组件")]
    public Image crosshairSmall; // 小圆准星（静态）
    public Image crosshairBig;   // 大同心圆（动态缩放）
    private float Mulitplier;
    private float _time;
    private bool _isCritical = false;

    [Header("周期动画参数")]
    public float idleAnimCycle = 0.5f; // 缩放周期（和全局计时器同步）
    public float maxScale = 1.5f;      // 最大缩放比例（圆环最大半径）
    private float _scaleProgress = 0f; // 缩放进度（0~1）
    private Vector3 _originBigScale;   // 外圈原始缩放大小


    // 图片路径常量（Resources目录下的相对路径，无需后缀）
    private const string BigCirclePath = "PicUI/Circle"; 

    private void CrosshairInit()
    {
        Debug.Log("UICrosshair: Initializing crosshair...");

        // 1. 加载外圈图片（核心逻辑）
        LoadBigCircleSprite();

        // 2. 初始化准星基础状态
        crosshairSmall.enabled = true;
        crosshairBig.enabled = true;

        // 3. 保存外圈原始缩放（必须在图片加载后执行）
        if (crosshairBig != null)
        {
            _originBigScale = crosshairBig.transform.localScale;
        }

        Debug.Log($"UICrosshair: Initialized at _time {_time:F1} seconds");
    }
    private void LoadBigCircleSprite()
    {
        // 空值保护
        if (crosshairBig == null)
        {
            Debug.LogError("UICrosshair: crosshairBig组件未赋值!");
            return;
        }

        // 加载Sprite（Resources.Load无需写后缀，路径是Resources下的相对路径）
        Sprite bigCircleSprite = Resources.Load<Sprite>(BigCirclePath);
        if (bigCircleSprite == null)
        {
            Debug.LogError($"UICrosshair: 未找到图片资源,路径:Resources/{BigCirclePath}.png");
            return;
        }

        // 赋值给外圈Image的sprite
        crosshairBig.sprite = bigCircleSprite;
        // 确保Image显示模式正确（适配图片大小）
        crosshairBig.type = Image.Type.Simple;
        crosshairBig.preserveAspect = true; // 保持图片宽高比，避免拉伸
        Debug.Log("UICrosshair: 外圈图片加载成功！");
    }

    //  精准命中动画
    private void AnimCriticalHit()
    {
        //  TODO:
        //  暂停普通状态动画，外圈变蓝色，持续0.05s
        //  重置动画状态，继续普通状态动画
    }
    private void AnimNormalHit()
    {
        //  TODO:
        //  暂停普通状态动画，外圈变红色，持续0.05s
        //  重置动画状态，继续普通状态动画
    }
    //  普通状态动画
    // private void AnimIdle()
    // {
    //     //  TODO:
    //     // 缩小的动画效果，准星一个半径更大的同心圆，逐渐缩小到准星大小
    // }

    // 普通状态动画（大圆环缩放核心逻辑）
    private void AnimIdle()
    {
        // 空值保护
        if (crosshairBig == null) return;
        // 1. 更新缩放进度[0, 1)
        _scaleProgress += Time.deltaTime / idleAnimCycle;
        // 2. 循环
        if (_scaleProgress >= 1f)
        {
            _scaleProgress = 0f;
        }
        // 3. 线性插值
        float currentScale = Mathf.Lerp(maxScale, 1f, _scaleProgress);
        // 4. 应用缩放到外圈圆环（仅缩放，不影响位置/旋转）
        crosshairBig.transform.localScale = _originBigScale * currentScale;
    }

    private void OnEnemyHit(EnemyHitEvent evt)
    {
        if (_isCritical)
        {
            AnimCriticalHit();
        }
        else
        {
            AnimNormalHit();
        }
    }
    private void OnTimerOnline(TimerOnlineEvent evt)
    {
        //  TODO:
        _time = evt.time;
        Debug.Log("UICrosshair:Received TimerOnlineEvent");
    }

    // 全局伤害倍率变化的回调方法
    private void OnMultiplierChanged(GlobalAttackMultiplierChangedEvent evt)
    {
        _isCritical = evt.isCritical;
    }

    //  更新准星位置到鼠标位置
    private void UpdateCrosshairToMouse()
    {
        Vector2 pos = Input.mousePosition;
        transform.position = pos;
    }

    void OnEnable()
    {
        EventBus.Instance.Subscribe<TimerOnlineEvent>(OnTimerOnline);
        EventBus.Instance.Subscribe<GlobalAttackMultiplierChangedEvent>(OnMultiplierChanged);
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe<TimerOnlineEvent>(OnTimerOnline);
        EventBus.Instance.Unsubscribe<GlobalAttackMultiplierChangedEvent>(OnMultiplierChanged);
        EventBus.Instance.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
    }

    private void Awake()
    {
        crosshairSmall = transform.Find("CrosshairSmall")?.GetComponent<Image>();
        crosshairBig = transform.Find("CrosshairBig")?.GetComponent<Image>();
        if (crosshairBig != null)
        {
            _originBigScale = crosshairBig.transform.localScale;
        }
    }
    void Start()
    {
        CrosshairInit();
    }
    void Update()
    {
        UpdateCrosshairToMouse();
        AnimIdle();
    }
    // private void OnDestroy()
    // {
    //     StopAllCoroutines(); // 停止所有动画协程
    // }
}