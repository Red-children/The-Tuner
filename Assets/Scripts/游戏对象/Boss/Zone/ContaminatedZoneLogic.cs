using UnityEngine;

public class ContaminatedZoneLogic : MonoBehaviour
{
    private Transform player;
    private float damageTimer;

    public float damagePerSecond = 10f;

    private ZoneCoreEnemy core;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        core = GetComponentInChildren<ZoneCoreEnemy>();
        Debug.Log("核心是否找到：" + core);
    }

    void Update()
    {
        if (core == null) return; // 核心死 → 区域失效

        if (player == null) return;

        if (IsPlayerInside())
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= 1f)
            {
                damageTimer = 0f;
                DealDamage(player.gameObject);
            }
        }
    }

    bool IsPlayerInside()
    {
        return Vector3.Distance(transform.position, player.position) <= 5f;
    }

    void DealDamage(GameObject target)
    {
        var dmg = target.GetComponent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(damagePerSecond);
        else
            target.SendMessage("TakeDamage", damagePerSecond, SendMessageOptions.DontRequireReceiver);
    }
}