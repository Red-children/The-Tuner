using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public float health = 100f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float moveSpeed = 2f;

    //冷却（由技能调用）
    public void OnSkillUsed()
    {
        Debug.Log("Boss used a skill");
    }

    public void Attack()
    {
        Debug.Log("Boss Attack");

        // 可加入攻击逻辑
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log("Boss took damage: " + damage);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss Died");
        Destroy(gameObject);
    }
}