using System;
using UnityEngine;
using UnityEngine.UI;

// 准星主控脚本（挂载在UICrosshair对象上）
public class UICrosshairController : MonoBehaviour
{
    // 子脚本引用（自动获取，无需手动赋值）
    // private CrosshairSpriteLoader _spriteLoader;
    // private CrosshairAnimator _animator;
    public CrosshairSpriteLoader _spriteLoader;
    public CrosshairAnimator _animator;

    // BGM相关数据（转发给子脚本）
    private double _dspStartTime;
    private bool _isCritical = false;

    #region 生命周期
    private void Awake()
    {
        // 自动获取子脚本（必须挂载在同一对象上）
        _spriteLoader = gameObject.GetComponent<CrosshairSpriteLoader>();
        _animator = gameObject.GetComponent<CrosshairAnimator>();
        Debug.Log($"_spriteLoader:{_spriteLoader}");
        Debug.Log($"_animator:{_animator}");
        // Debug.Log($"UICrosshairController: _spriteLoader={_spriteLoader != null}, _animator={_animator != null}");
        // 检查子脚本是否存在
        if (_spriteLoader == null)
        {
            Debug.LogError("UICrosshairController: 未找到CrosshairSpriteLoader脚本!");
        }
        if (_animator == null)
        {
            Debug.LogError("UICrosshairController: 未找到CrosshairAnimator脚本!");
        }
    }

    private void Start()
    {
        // 初始化子脚本（加载图片）
        _spriteLoader?.InitCrosshairSprites();
    }

    private void OnEnable()
    {
        // 订阅所有事件（主控统一订阅，避免子脚本重复订阅）
        Debug.Log("UICrosshairController: 开始订阅事件");
        PreciseEventBus.Instance.Subscribe<IndicatorActiveEvent>(OnIndicatorActive);
        PreciseEventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);
        PreciseEventBus.Instance.Subscribe<BGMProgressUpdateEvent>(OnProgressUpdate);
        EventBus.Instance.Subscribe<AttackMultiplierChangedEvent>(OnMultiplierChanged);
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
        Debug.Log("UICrosshairController: 事件订阅完成");
    }

    private void OnDisable()
    {
        // 取消所有事件订阅
        PreciseEventBus.Instance.Unsubscribe<IndicatorActiveEvent>(OnIndicatorActive);
        PreciseEventBus.Instance.Unsubscribe<PlayBGMEvent>(OnPlayBGM);
        PreciseEventBus.Instance.Unsubscribe<BGMProgressUpdateEvent>(OnProgressUpdate);
        EventBus.Instance.Unsubscribe<AttackMultiplierChangedEvent>(OnMultiplierChanged);
        EventBus.Instance.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
    }

    private void Update()
    {
        // 准星跟随鼠标（主控负责基础交互）
        UpdateCrosshairToMouse();
        // 测试逻辑（仅调试用）
        TestTemp();
        // 通知动画脚本更新闲置状态
        _animator?.UpdateIdleState();
    }
    #endregion

    #region 核心逻辑
    // 准星跟随鼠标位置
    private void UpdateCrosshairToMouse()
    {
        Vector2 pos = Input.mousePosition;
        transform.position = pos;
    }

    // 测试用：鼠标左键触发命中事件
    private void TestTemp()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnEnemyHit(new EnemyHitEvent());
        }
    }
    #endregion

    #region 事件回调（主控转发给子脚本）
    // 接收BGM播放事件：记录开始时间
    private void OnPlayBGM(PlayBGMEvent evt)
    {
        _dspStartTime = evt.time;
        // 转发给动画脚本
        _animator?.SetDspStartTime(_dspStartTime);
    }

    // 接收倍率变化事件：更新暴击状态
    private void OnMultiplierChanged(AttackMultiplierChangedEvent evt)
    {
        _isCritical = evt.isCritical;
        // 转发给动画脚本
        _animator?.SetCriticalState(_isCritical);
    }

    // 接收敌人命中事件：触发命中动画
    private void OnEnemyHit(EnemyHitEvent evt)
    {
        double currentTime = AudioSettings.dspTime - _dspStartTime;
        // 通知动画脚本播放命中动画
        _animator?.PlayHitAnimation(_isCritical, currentTime);
    }

    // 接收BGM进度更新事件：转发进度
    private void OnProgressUpdate(BGMProgressUpdateEvent evt)
    {
        _animator?.UpdateBgmProgress(evt.PreciseTime);
    }

    // 接收指示器激活事件：触发准星缩放动画
    private void OnIndicatorActive(IndicatorActiveEvent evt)
    {
        // 通知动画脚本播放缩放动画
        Debug.Log($"UICrosshairController: 接收到IndicatorActiveEvent！nextPoint={evt.nextPoint}, time={evt.time}"); // 新增日志
        _animator?.PlayScaleAnimation(evt);
    }
    #endregion
}