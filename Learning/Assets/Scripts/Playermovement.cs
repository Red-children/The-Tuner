using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Playermovement : MonoBehaviour
{
    //tag
    public string playerTag = "Player";
    public float speed = 5; //移动速度
    public float facingDirection = 1;   //朝向
    public Rigidbody2D rb;
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        //设置tag
        gameObject.tag = playerTag;
    }
    // Update is called once per frame
    // void Update()
    // {
    // }

    void FixedUpdate()
    {
        //  键盘输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        //  动画参数绑定
        anim.SetFloat("Horizontal", Mathf.Abs(horizontal));
        anim.SetFloat("Vertical", Mathf.Abs(vertical));

        //Turning back
        if(horizontal != 0)
        {
            facingDirection = Mathf.Sign(horizontal);
            transform.localScale = new Vector3(facingDirection, 1, 1);

            // transform.rotation = Quaternion.Euler(0, 0, 0);

        }

        rb.velocity = new Vector2(horizontal, vertical) * speed;
    }
}
