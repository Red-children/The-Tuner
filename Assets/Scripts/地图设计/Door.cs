using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Door : MonoBehaviour
{
    private Collider2D doorCollider;
    private Animator animator;
    public enum Direction { Up, Down, Left, Right }

    [Header("门的开关方向")]
    public Direction direction;   



    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        Open();
    }

    /// <summary>
    /// 开门，禁用碰撞体并播放开门动画
    /// </summary>
    public void Open()
    {
        doorCollider.enabled = false;       //关闭碰撞体，使玩家可以通过
        if (animator != null)               //播放开门动画，动画参数根据方向设置
            animator.SetBool("isOpen", true);
    }

    /// <summary>
    /// 关门，启用碰撞体并播放关门动画
    /// </summary>
    public void Close()                     //启用碰撞体，阻止玩家通过，并播放关门动画
    {
        doorCollider.enabled = true;        //启用碰撞体，阻止玩家通过
        if (animator != null)
            animator.SetBool("isOpen", false);
    }
}