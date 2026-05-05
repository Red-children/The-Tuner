using UnityEngine;

public class EnemyRangedWeapon : WeaponInfo
{
    [Header("敌人远程武器")]
    public float projectileSpeed = 8f;
    public GameObject projectilePrefab;

    protected override void Awake()
    {
        base.Awake();
        owner = WeaponOwner.Enemy;
    }

    public override void Fire(float damage, float rhythmMultiplier, RhythmRank rank)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 敌人远程武器缺少 projectilePrefab");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 敌人远程武器缺少 firePoint");
            return;
        }

        // 创建弹丸
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // 设置弹丸方向（朝向玩家）
        EnemyController enemyController = GetComponentInParent<EnemyController>();
        if (enemyController != null && enemyController.runtime.target != null)
        {
            Vector2 direction = (enemyController.runtime.target.position - firePoint.position).normalized;
            
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * projectileSpeed;
            }

            // 设置伤害
            EnemyWeaponHit hitScript = projectile.GetComponent<EnemyWeaponHit>();
            if (hitScript != null)
            {
                hitScript.owner = enemyController;
                hitScript.damage = (int)damage;
            }
        }
    }
}
