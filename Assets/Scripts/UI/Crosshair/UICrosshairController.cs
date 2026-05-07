using UnityEngine;

public class UICrosshairController : MonoBehaviour
{
    [Header("子组件")]
    [SerializeField] private CrosshairAnimator _animator;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<CrosshairAnimator>();
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
        Debug.Log("Crosshair Receive Event Mouse Down");
        Debug.Log($"rank == {evt.rank}");
        // if (RhythmManager.Instance == null) return;
        if (_animator == null) return;
        // RhythmRank rank = RhythmManager.Instance.GetRank().rank;
        // _animator.PlayHitAnimation(rank);
        _animator.PlayHitAnimation(evt.rank);
    }
}