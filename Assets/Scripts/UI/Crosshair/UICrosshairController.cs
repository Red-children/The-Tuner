using UnityEngine;

public class UICrosshairController : MonoBehaviour
{
    [Header("子组件")]
    [SerializeField] private CrosshairSpriteLoader _spriteLoader; //图片加载器
    [SerializeField] private CrosshairAnimator _animator;         //动画控制子脚本

    [SerializeField] private double _dspStartTime;                //记录开始时间

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
        EventBus.Instance.Subscribe<EnemyHitEvent>(OnEnemyHit);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
    }

    private void Update()
    {
        transform.position = Input.mousePosition; // 准星跟随鼠标
    }

    private void OnEnemyHit(EnemyHitEvent evt)
    {
        RhythmRank rank = RhythmManager.Instance.GetRank(AudioSettings.dspTime).rank;
        _animator?.PlayHitAnimation(rank);
    }
}