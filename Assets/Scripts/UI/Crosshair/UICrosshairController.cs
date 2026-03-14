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
    // private double _dspStartTime;
    public double _dspStartTime;
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
        // 订阅你的节奏事件
        EventBus.Instance.Subscribe<BeatPreviewEvent>(OnBeatPreview);
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit); // 命中事件如果还用就保留
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<BeatPreviewEvent>(OnBeatPreview);
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
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
   

    
    // 接收敌人命中事件：触发命中动画
    private void OnEnemyHit(EnemyHitEvent evt)
    {
        double currentTime = AudioSettings.dspTime - _dspStartTime;
        // 通知动画脚本播放命中动画
        _animator?.PlayHitAnimation(_isCritical, currentTime);
    }

    // 接收BGM进度更新事件：转发进度
    
   
    #endregion

    private void OnBeatPreview(BeatPreviewEvent evt)
    {
        _animator?.PlayScaleAnimation(evt);
    }

    private void OnRhythmData(RhythmData data)
    {                                         
        // 根据节奏数据更新暴击状态
        _isCritical = data.rank == RhythmRank.Perfect || data.rank == RhythmRank.Great;
        _animator?.SetCriticalState(_isCritical);
    }
}