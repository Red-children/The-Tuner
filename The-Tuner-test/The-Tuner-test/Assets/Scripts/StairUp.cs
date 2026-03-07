using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;
//  上楼梯
public class StairUp : MonoBehaviour
{

    // public string moutainsKey = "Moutains";
    // public string boundarysKey = "Boundarys";
    public Collider2D[] mountains;
    public Collider2D[] boundarys;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            foreach(Collider2D mountain in mountains)
            {
                mountain.enabled = false;
            }
            foreach(Collider2D boundary in boundarys)
            {
                boundary.enabled = true;
            }
            other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 15;
        }
    }
}
