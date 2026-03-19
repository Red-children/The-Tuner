using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Door : MonoBehaviour
{
    private Collider2D doorCollider;
    private Animator animator;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        Open();
    }

    /// <summary>开门：禁用碰撞器，播放开门动画（如果有）</summary>
    public void Open()
    {
        doorCollider.enabled = false;
        if (animator != null)
            animator.SetBool("isOpen", true);
    }

    /// <summary>关门：启用碰撞器，播放关门动画（如果有）</summary>
    public void Close()
    {
        doorCollider.enabled = true;
        if (animator != null)
            animator.SetBool("isOpen", false);
    }
}