using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // 模块引用
    public PlayerMovement movement;
    public PlayerAttack attack;
    public PlayerDash dash;

    // 可选的动画模块，不需禁用
    //public PlayerAnimation animation;

    public bool isDead = false;

    private void Awake()
    {
        // 自动获取模块（如果未拖拽）
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (attack == null) attack = GetComponent<PlayerAttack>();
        if (dash == null) dash = GetComponent<PlayerDash>();
       // if (animation == null) animation = GetComponent<PlayerAnimation>();
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<PlayerHurtEvent>(OnPlayerHurt);
        EventBus.Instance.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<PlayerHurtEvent>(OnPlayerHurt);
        EventBus.Instance.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnPlayerHurt(PlayerHurtEvent evt)
    {
        if (isDead) return;

        // 受击时禁用移动、攻击、冲刺模块（动画继续）
        movement.enabled = false;
        attack.enabled = false;
        dash.enabled = false;

        StartCoroutine(RecoverFromHit());
    }

    private IEnumerator RecoverFromHit()
    {
        yield return new WaitForSeconds(0.5f); // 硬直时间，可配置
        if (!isDead)
        {
            movement.enabled = true;
            attack.enabled = true;
            dash.enabled = true;
        }
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        isDead = true;
        // 永久禁用模块
        movement.enabled = false;
        attack.enabled = false;
        dash.enabled = false;
        // 可触发游戏结束界面等
    }

}
