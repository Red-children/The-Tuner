using UnityEngine;

public class ZoneCoreEnemy : MonoBehaviour
{
    public float hp = 30f;

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        Debug.Log("核心受击: " + dmg);

        if (hp <= 0)
        {
            // 销毁整个污染区域（父物体）
            Destroy(transform.parent.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            TakeDamage(bullet.damage);
            bullet.DestroyMyself();  
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null)
        {
            TakeDamage(bullet.damage);
            bullet.DestroyMyself(); 
        }
    }
}