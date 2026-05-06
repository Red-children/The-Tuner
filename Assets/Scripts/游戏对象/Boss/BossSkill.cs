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

        int random;

        // 一阶段 
        if (runtime.currentPhase == 1)
        {
            random = Random.Range(0, 1); 
        }
        // 二阶段 
        else
        {
            random = Random.Range(0, 2); 
        }

        switch (random)
        {
            case 0:
                SummonContaminatedZone();
                break;

            case 1:
                ShockwaveAttack();
                break;
        }

        lastUseTime = Time.time;
    }


    private void SummonContaminatedZone()
    {
        Debug.Log("使用技能1");

        Vector3 spawnPos = transform.position + (Vector3)(Random.insideUnitCircle * 5f);

        GameObject zoneObj = Instantiate(runtime.Data.contaminatedZonePrefab, spawnPos, Quaternion.identity);

        var logic = zoneObj.GetComponent<ContaminatedZoneLogic>();
        Debug.Log("逻辑组件：" + logic);

        if (logic != null)
        {
            logic.Initialize(runtime.Data, this);
        }

        Debug.Log("生成污染区域：" + zoneObj);

        lastUseTime = Time.time;
    }

    void ShockwaveAttack()
    {
        Debug.Log("使用技能：冲击波");

        if (runtime.target == null) return;

        // 停止移动
        runtime.currentMoveSpeed = 0f;
        runtime.currentChaseSpeed = 0f;

        Transform player = runtime.target;

        Vector3 dir = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dir,
            distance,
            LayerMask.GetMask("Wall")
        );

        if (hit.collider == null)
        {
            DealDamageToPlayer(player.gameObject);
            Debug.Log("冲击波命中玩家");
        }
        else
        {
            Debug.Log("冲击波被墙挡住");
        }
    }

    public void RemoveZone(GameObject zone)
    {
        if (activeZones.Contains(zone))
            activeZones.Remove(zone);
    }

    void DealDamageToPlayer(GameObject playerObj)
    {
        PlayerHealth player = playerObj.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage((int)runtime.Data.specialAttackDamage);
        }
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