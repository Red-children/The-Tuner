using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // ģ���
    public PlayerMovement movement;
    public PlayerAttack attack;
    public PlayerDash dash;

    // ��ѡ�Ķ���ģ�飬�������
    //public PlayerAnimation animation;

    public bool isDead = false;

    private void Awake()
    {
        // �Զ���ȡģ�飨���δ��ק��
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

        // �ܻ�ʱ�����ƶ������������ģ�飨����������
        movement.enabled = false;
        attack.enabled = false;
        dash.enabled = false;

        StartCoroutine(RecoverFromHit());
    }

    private IEnumerator RecoverFromHit()
    {
        yield return new WaitForSeconds(0.5f); // Ӳֱʱ�䣬������
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
        // ���ý���ģ��
        movement.enabled = false;
        attack.enabled = false;
        dash.enabled = false;
        // �ɴ�����Ϸ���������
    }

}
