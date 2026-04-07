using UnityEngine;

public class BossSkill : MonoBehaviour
{
    public float pullDistance = 1.5f;
    public float skillCooldown = 5f;

    private float lastUseTime;

    public void UseRandomSkill(BossEnemy boss, Transform target)
    {
        int random = Random.Range(0, 2);

        if (random == 0)
        {
            Teleport(boss, target);
        }
        else
        {
            PullPlayer(boss, target);
        }
    }

    public bool CanUseSkill()
    {
        return Time.time >= lastUseTime + skillCooldown;
    }

    public void OnUseSkill()
    {
        lastUseTime = Time.time;
    }

    void Teleport(BossEnemy boss, Transform target)
    {
        Vector3 dir = (boss.transform.position - target.position).normalized;
        boss.transform.position = target.position + dir * 3f;

        Debug.Log("Teleport");
    }

    void PullPlayer(BossEnemy boss, Transform target)
    {
        Vector3 dir = (boss.transform.position - target.position).normalized;

        Rigidbody rb = target.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.MovePosition(boss.transform.position - dir * pullDistance);
        }
        else
        {
            target.position = boss.transform.position - dir * pullDistance;
        }

        Debug.Log("Pull Player");
    }
}