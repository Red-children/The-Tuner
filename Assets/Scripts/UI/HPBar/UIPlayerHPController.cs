using UnityEngine;

//  主控
public class UIPlayerHPController : MonoBehaviour
{
    [Header("子模块")]
    public UIHPBinder binder;
    public UIHPAnimation animationPlayer;

    private float _maxHP;
    private float _lastHP;

    private void Awake()
    {
        binder = GetComponent<UIHPBinder>();
        animationPlayer = GetComponent<UIHPAnimation>();
    }

    private void Start()
    {
        binder.InitComponents();
        // 修复：主动给动画层传递引用
        animationPlayer.Init(binder);
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<PlayerHealthChangedEventStruct>(OnHPChanged);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<PlayerHealthChangedEventStruct>(OnHPChanged);
    }

    private void OnHPChanged(PlayerHealthChangedEventStruct evt)
    {
        if (Mathf.Abs(_maxHP - evt.maxHealth) > 1e-6)
            _maxHP = evt.maxHealth;

        _lastHP = evt.currentHealth;

        animationPlayer.SetHPPercent(evt.healthPercent);
        binder.SetHPText($"{_lastHP} / {_maxHP}");
    }
}