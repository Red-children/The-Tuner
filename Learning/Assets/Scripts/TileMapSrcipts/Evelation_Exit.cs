using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evelation_Exit : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundaryColliders;
    // Start is called before the first frame update

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            foreach(Collider2D mountain in mountainColliders)
            {
                mountain.enabled = true;
            }
            foreach(Collider2D boundary in boundaryColliders)
            {
                boundary.enabled = false;
            }
            //  恢复玩家排序层级
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
        }

    }
}
