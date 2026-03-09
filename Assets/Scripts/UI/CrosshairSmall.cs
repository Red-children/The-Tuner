using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  中心准星，左键变色
public class CrosshairSmall : MonoBehaviour
{
    public Animator anim;
    public string animStateName = "PreciseHit";
    private void TriggerTest()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.Play(animStateName, 0, 0);
            Debug.Log("Play Animation:PreciseHit");
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