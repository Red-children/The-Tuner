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
    private float time;

    public struct TimerOnlineEvent
    {
        //  TODO:
        public float time; // 事件发生时间
    }
    public struct EnemyHitEvent
    {
        //  TODO:
    }
    public struct PlayerAtkEvent
    {
        //  TODO:
        // public bool isCritical; // 是否暴击（可选）
        // public float damage;    // 造成的伤害（可选）
    }

    private void CrosshairInit()
    {
        Debug.Log("UICrosshair: Initializing crosshair...");

                // 初始化准星基础状态
        crosshairSmall.enabled = true;
        crosshairBig.enabled = true;
        // crosshairSmall.color = _originSmallColor;
        // crosshairBig.color = _originBigColor;

        Debug.Log($"UICrosshair: Initialized at time {time:F1} seconds");
    }
    //  精准命中动画
    private void AnimSucess()
    {
        //  TODO:
        //  暂停普通状态动画，显示变色效果，持续0.05s
        //  重置动画状态，继续普通状态动画
    }
    //  普通状态动画
    private void AnimIdle()
    {
        //  TODO:
        // 缩小的动画效果，准星一个半径更大的同心圆，逐渐缩小到准星大小
    }
    private void OnEnemyHit(EnemyHitEvent evt)
    {
        //  播放命中动画
        AnimSucess();

    }
    private void OnTimerOnline(TimerOnlineEvent evt)
    {
        //  TODO:
        time = evt.time;
    }

    // 全局伤害倍率变化的回调方法
    private void OnMultiplierChanged(GlobalAttackMultiplierChangedEvent evt)
    {
        Mulitplier = evt.newMultiplier;
    }

    private void UpdateCrosshairToMouse()
    {
        //  TODO:
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
        EventBus.Instance.Unsubscribe<GlobalAttackMultiplierChangedEvent>(OnMultiplierChanged);
        EventBus.Instance.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
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
}