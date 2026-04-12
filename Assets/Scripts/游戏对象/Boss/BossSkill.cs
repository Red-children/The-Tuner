using UnityEngine;

public class BossSkill : MonoBehaviour
{
    private BossRuntime runtime;
    [HideInInspector] public float lastUseTime;

    void Awake()
    {
        runtime = GetComponent<BossRuntime>();
    }

    public bool CanUseSkill()
    {
        if (runtime == null || runtime.Data == null) return false;
        return Time.time >= lastUseTime + runtime.Data.skillCooldown;
    }

    public void UseRandomSkill()
    {
        if (runtime.target == null) return;

        int random = Random.Range(0, 2);
        if (random == 0)
            Teleport();
        else
            PullPlayer();

        lastUseTime = Time.time;
    }

    void Teleport()
    {
        Vector3 dir = (transform.position - runtime.target.position).normalized;
        transform.position = runtime.target.position + dir * runtime.Data.teleportOffset;
    }

    void PullPlayer()
    {
        Vector3 dir = (transform.position - runtime.target.position).normalized;
        Rigidbody rb = runtime.target.GetComponent<Rigidbody>();
        if (rb) rb.MovePosition(transform.position - dir * runtime.Data.pullDistance);
        else runtime.target.position = transform.position - dir * runtime.Data.pullDistance;
    }
}