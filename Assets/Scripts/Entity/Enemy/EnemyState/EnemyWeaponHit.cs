using UnityEngine;

public class EnemyWeaponHit : MonoBehaviour
{
    public EnemyController owner;          // 所属敌人
    public int damage;                     // 伤害值
    public float hitCooldown = 0.5f;       // 同一激活周期内只触发一次伤害

    private bool canHit = true;
    private bool hasHitInCurrentActivation = false;

    private void OnEnable()
    {
        hasHitInCurrentActivation = false;
        canHit = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHit || hasHitInCurrentActivation) return;
        if (!other.CompareTag("Player")) return;

        PlayerAPI player = other.GetComponent<PlayerAPI>();
        if (player != null)
        {
            player.TakeDamage(damage);
            hasHitInCurrentActivation = true;
            Debug.Log($"敌人武器命中玩家，造成 {damage} 点伤害");
        }
    }
    
    public void ResetHitFlag()
    {
        hasHitInCurrentActivation = false;
        canHit = true;
    }

}