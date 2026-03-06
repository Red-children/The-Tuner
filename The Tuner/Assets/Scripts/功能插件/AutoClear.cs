using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoClear : MonoBehaviour
{
    public int time = 2;
    void Start()
    {
        Destroy(gameObject, time);
    }


}
