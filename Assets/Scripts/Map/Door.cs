using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Door : MonoBehaviour
{
    private Collider2D doorCollider;
    private Animator animator;

    [Header("初始状态")]
    [SerializeField] private bool startOpen = false; // 默认改为 false

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        
        if (startOpen)
            Open();
        else
            Close();
    }

    public void Open()
    {
        doorCollider.enabled = false;
        if (animator != null)
            animator.SetBool("isOpen", true);
    }

    public void Close()
    {
        doorCollider.enabled = true;
        if (animator != null)
            animator.SetBool("isOpen", false);
    }
}