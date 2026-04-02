using UnityEngine;
using UnityEngine.PlayerLoop;

//  主控
public class UIPlayerHPController : MonoBehaviour
{
    [Header("子模块")]
    public UIHPBinder binder;
    public UIHPAnimation animationPlayer;

    private float _maxHP;
    private float _lastHP;

    private void Init()
    {
        binder = GetComponent<UIHPBinder>();
        animationPlayer = GetComponent<UIHPAnimation>();
    }
    private void RefreshHP()
    {
        PlayerStats player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        if(player == null) return;
        _maxHP = player.MaxHealth;
        _lastHP = player.CurrentHealth;
        // 直接刷新视图
        animationPlayer.SetHPPercent(_lastHP / _maxHP);
        binder.SetHPText($"{_lastHP} / {_maxHP}");
    }
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        binder.InitComponents();
        //  主动给动画层传递引用
        animationPlayer.Init(binder);
        //  等一帧
        Invoke(nameof(RefreshHP), 0.1f);
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
        if (binder == null || animationPlayer == null) return;
        if (Mathf.Abs(_maxHP - evt.maxHealth) > 1e-6)
            _maxHP = evt.maxHealth;

        _lastHP = evt.currentHealth;

        animationPlayer.SetHPPercent(evt.healthPercent);
        binder.SetHPText($"{_lastHP} / {_maxHP}");
    }
}