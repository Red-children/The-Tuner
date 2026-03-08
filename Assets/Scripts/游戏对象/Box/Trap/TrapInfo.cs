using UnityEngine;
using System.Collections;

public class TrapInfo : MonoBehaviour
{
    public TrapBase trapBase;
    public TrapType trapType;

    private TrapStats stats;

    private bool spikeActive = false;

    void Start()
    {
        stats = trapBase.GetTrapStats(trapType);

        if (trapType == TrapType.Spike)
        {
            StartCoroutine(SpikeCycle());
        }
    }

    IEnumerator SpikeCycle()
    {
        while (true)
        {
            spikeActive = false;

            yield return new WaitForSeconds(stats.inactiveTime);

            spikeActive = true;

            yield return new WaitForSeconds(stats.activeTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (stats == null) return;

        switch (trapType)
        {
            case TrapType.Spike:
                HandleSpike(other);
                break;

            case TrapType.SlowFloor:
                HandleSpeedEffect(other, stats.speedMultiplier);
                break;

            case TrapType.SpeedFloor:
                HandleSpeedEffect(other, stats.speedMultiplier);
                break;
        }
    }

    void HandleSpike(Collider2D other)
    {
        if (!spikeActive) return;

        var components = other.GetComponents<MonoBehaviour>();

        foreach (var comp in components)
        {
            var method = comp.GetType().GetMethod("TakeDamage");

            if (method != null)
            {
                method.Invoke(comp, new object[] { stats.damage });
                break;
            }
        }
    }

    void HandleSpeedEffect(Collider2D other, float multiplier)
    {
        Debug.Log("´¥·¢¼õËÙÏÝÚå");

        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            StartCoroutine(SpeedBuff(rb, multiplier, stats.buffDuration));
        }
    }

    IEnumerator SpeedBuff(Rigidbody2D rb, float multiplier, float duration)
    {
        float originalDrag = rb.drag;

        rb.drag *= (1 / multiplier);

        Debug.Log("Drag changed to: " + rb.drag);

        yield return new WaitForSeconds(duration);

        rb.drag = originalDrag;
    }
}