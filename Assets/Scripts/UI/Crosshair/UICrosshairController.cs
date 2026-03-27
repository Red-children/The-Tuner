using UnityEngine;

public class UICrosshairController : MonoBehaviour
{
    [Header("子组件")]
    public CrosshairSpriteLoader _spriteLoader; //图片加载器
    public CrosshairAnimator _animator;         //动画控制子脚本

    public double _dspStartTime;                //记录开始时间
    private bool _isCritical = false;           // 当前是否为精准窗口（Perfect/Great），用于命中动画

    private void Awake()
    {
        _spriteLoader = GetComponent<CrosshairSpriteLoader>();                                
        _animator = GetComponent<CrosshairAnimator>();
        if (_spriteLoader == null) Debug.LogError("缺少 CrosshairSpriteLoader");
        if (_animator == null) Debug.LogError("缺少 CrosshairAnimator");
    }

    private void Start()
    {
        _spriteLoader?.InitCrosshairSprites();  //初始化图片
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);  //订阅节奏变化事件得到对应的数据
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
        EventBus.Instance.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
    }

    private void Update()
    {
        transform.position = Input.mousePosition; // 准星跟随鼠标
        TestTemp(); // 仅测试用
    }

    private void TestTemp()
    {
        if (Input.GetMouseButtonDown(0))
            OnEnemyHit(new EnemyHitEvent());
    }

    private void OnEnemyHit(EnemyHitEvent evt)
    {
        double currentTime = AudioSettings.dspTime - _dspStartTime;
        _animator?.PlayHitAnimation(_isCritical, currentTime);
    }

    //得到传递来的数据 现在通过我的RhythmManager传递数据
    private void OnRhythmData(RhythmData data)
    {
        _isCritical = data.rank == RhythmRank.Perfect || data.rank == RhythmRank.Great ||data.rank== RhythmRank.Great;
        _animator?.SetCriticalState(_isCritical);
    }
}