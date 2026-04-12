using UnityEngine;

public class BossRuntime : MonoBehaviour
{
    [Header("运行时状态")]
    public Transform target;
    public bool getHit;
    public float currentHealth;
    public float currentMoveSpeed;
    public float currentChaseSpeed;

    [Header("阶段状态")]
    public int currentPhase = 1;
    public bool hasPhaseChanged = false;

    [Header("数据访问")]
    [SerializeField] private BossData originalData;

    public BossData Data => originalData;

    public void Init(BossData bossData)
    {
        originalData = bossData;
        currentHealth = originalData.health;
        currentMoveSpeed = originalData.moveSpeed;
        currentChaseSpeed = originalData.chaseSpeed;
    }

    public float Health => currentHealth;
    public float MoveSpeed => currentMoveSpeed;
    public float ChaseSpeed => currentChaseSpeed;

    public float IdleTime => originalData.idleTime;
    public LayerMask TargetLayer => originalData.targetLayer;
    public GameObject DamageTextPrefab => originalData.damageTextPrefab;
    public GameObject DeadEff => originalData.deadEff;
    public float PatrolRadius => originalData.patrolRadius;
}