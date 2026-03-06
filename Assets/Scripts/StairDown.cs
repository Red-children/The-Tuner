using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;
//  下楼梯
public class StairDown : MonoBehaviour
{

    // public string moutainsKey = "Moutains";
    // public string boundarysKey = "Boundarys";
    public Collider2D[] moutains;
    public Collider2D[] boundarys;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            foreach(Collider2D mountain in moutains)
            {
                mountain.enabled = true;
            }
            foreach(Collider2D boundary in boundarys)
            {
                boundary.enabled = false;
            }
            other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
    }
}
