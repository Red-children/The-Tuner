using UnityEngine;

public class ContaminatedZoneLogic : MonoBehaviour
{
    private Transform player;
    private float damageTimer;

    public float damagePerSecond = 10f;

    private ZoneCoreEnemy core;

    public GameObject minionPrefab;   // 拖入小怪
    public float spawnInterval = 3f;  // 生成间隔
    public int maxMinionCount = 3;    // 最大数量

    private float spawnTimer;
    private int currentMinionCount;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        core = GetComponentInChildren<ZoneCoreEnemy>();

        Debug.Log("核心是否找到：" + core);
    }

    void Update()
    {
        if (core == null) return;

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

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            if (currentMinionCount < maxMinionCount)
            {
                SpawnMinion();
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

    void SpawnMinion()
    {
        if (minionPrefab == null)
        {
            Debug.LogWarning("未设置 minionPrefab！");
            return;
        }

        Vector2 offset = Random.insideUnitCircle * 2f;

        GameObject minion = Instantiate(
            minionPrefab,
            transform.position + (Vector3)offset,
            Quaternion.identity
        );

        currentMinionCount++;

        Debug.Log("生成小怪，当前数量：" + currentMinionCount);

        MinionDeathListener listener = minion.AddComponent<MinionDeathListener>();
        listener.Init(this);
    }

    public void OnMinionDead()
    {
        currentMinionCount--;
        currentMinionCount = Mathf.Max(0, currentMinionCount);

        Debug.Log("小怪死亡，当前数量：" + currentMinionCount);
    }

    private class MinionDeathListener : MonoBehaviour
    {
        private ContaminatedZoneLogic owner;

        public void Init(ContaminatedZoneLogic logic)
        {
            owner = logic;
        }

        void OnDestroy()
        {
            if (owner != null)
            {
                owner.OnMinionDead();
            }
        }
    }
}