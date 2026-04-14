using UnityEngine;
using System.Collections.Generic;

public class BossSkill : MonoBehaviour
{
    private BossRuntime runtime;
    [HideInInspector] public float lastUseTime;

    private List<GameObject> activeZones = new List<GameObject>();

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

        activeZones.RemoveAll(zone => zone == null);

        int skillCount = 3;
        int random = Random.Range(0, skillCount);
        switch (random)
        {
            case 0:
                Teleport();
                break;
            case 1:
                PullPlayer();
                break;
            case 2:
                SummonContaminatedZone();
                break;
        }

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

    // ========== 污染区域技能 ==========
    private void SummonContaminatedZone()
    {
        activeZones.RemoveAll(zone => zone == null);
        if (activeZones.Count >= runtime.Data.maxZoneCount)
        {
            Debug.Log("污染区域数量已达上限");
            return;
        }

        Vector3 spawnPos = transform.position + (Vector3)(Random.insideUnitCircle * 5f);

        GameObject zoneObj = new GameObject("ContaminatedZone");
        zoneObj.transform.position = spawnPos;
        zoneObj.tag = "ContaminatedZone";

        SpriteRenderer sr = zoneObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.8f, 0.2f, 0.8f, 0.5f);

        CircleCollider2D col = zoneObj.AddComponent<CircleCollider2D>();
        col.radius = 2f;
        col.isTrigger = true;

        var zoneLogic = zoneObj.AddComponent<ContaminatedZoneLogic>();
        zoneLogic.Initialize(runtime.Data, this);
        activeZones.Add(zoneObj);
    }

    public void RemoveZone(GameObject zone)
    {
        if (activeZones.Contains(zone))
            activeZones.Remove(zone);
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
    }

    // 内部逻辑类（不新建文件）
    private class ContaminatedZoneLogic : MonoBehaviour
    {
        private BossData data;
        private Transform player;
        private float currentHealth;
        private float damageTimer;
        private BossSkill owner;

        public void Initialize(BossData bossData, BossSkill skillOwner)
        {
            data = bossData;
            owner = skillOwner;
            currentHealth = data.zoneMaxHealth;
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            StartCoroutine(SpawnMinions());
        }

        void Update()
        {
            if (player == null) return;

            if (IsPlayerInside())
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= 1f)
                {
                    damageTimer -= 1f;
                    // 对玩家造成伤害（兼容多种方式）
                    DealDamageToPlayer(data.zoneDamagePerSecond);
                }
            }
            else
            {
                damageTimer = 0f;
            }
        }

        private void DealDamageToPlayer(float damage)
        {
            GameObject playerObj = player.gameObject;

            // 方式1：检查是否实现了 IDamageable 接口
            IDamageable damageable = playerObj.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                return;
            }

            // 方式2：尝试 SendMessage（玩家脚本需实现 TakeDamage 方法）
            playerObj.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        private bool IsPlayerInside()
        {
            Collider2D col = GetComponent<Collider2D>();
            return col != null && col.bounds.Contains(player.position);
        }

        private System.Collections.IEnumerator SpawnMinions()
        {
            while (currentHealth > 0)
            {
                yield return new WaitForSeconds(data.zoneSpawnInterval);
                if (data.minionPrefab != null)
                {
                    Instantiate(data.minionPrefab, transform.position + Random.insideUnitSphere * 2f, Quaternion.identity);
                }
            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                if (owner != null)
                    owner.RemoveZone(gameObject);
                Destroy(gameObject);
            }
        }

        // 用于 SendMessage 接收玩家攻击伤害
        void ApplyDamage(float damage)
        {
            TakeDamage(damage);
        }
    }
}

// 如果需要接口，可以放在此文件末尾（不新建文件）
public interface IDamageable
{
    void TakeDamage(float damage);
}