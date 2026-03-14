using UnityEngine;

public class DstrObjInfo : MonoBehaviour
{
    public DstrObjBase objBase;
    public DstrObjType objType;

    private DstrObjStats stats;

    private float currentHealth;

    void Start()
    {
        stats = objBase.GetObjStats(objType);

        if (stats == null)
        {
            Debug.LogError("Object stats not found: " + objType);
            return;
        }

        currentHealth = stats.maxHealth;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = stats.objectSprite;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            DestroyObject();
        }
    }

    void DestroyObject()
    {
        if (stats.destroyEffect != null)
        {
            Instantiate(stats.destroyEffect, transform.position, Quaternion.identity);
        }
        if (stats.spawnUnit != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(stats.spawnUnit, spawnPos, Quaternion.identity);
        }

        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        Destroy(gameObject);
    }
}