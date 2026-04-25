using UnityEngine;

public class ZoneCoreEnemy : MonoBehaviour
{
    public float hp = 30f;

    void Update()
    {
        // 可选：闪烁/提示
    }

    public void TakeDamage(float dmg)
    {
        hp -= dmg;

        if (hp <= 0)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}