using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairBig : MonoBehaviour
{
    public Animator anim;
    private void TriggerTest()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("Trigger");
            Debug.Log("MouseButtonDown");
        }
    }

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
    }
    void Update()
    {
        TriggerTest();
    }
}
