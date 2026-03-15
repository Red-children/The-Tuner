using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerIdleState : IState
{
    private PlayerFSM manager;
    private PlayerParameter parameter;
    

    public PlayerIdleState(PlayerFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入Idle状态");
        
    }

    public void OnUpdate()
    {
        // 如果受到攻击，立即切换到受击状态
        if (parameter.getHit)
        {
            //manager.ChangeState(StateType.Wound);
            return;
        }
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        if(moveX != 0 || moveY != 0)
        {
            manager.ChangeState(E_PlayerState.Walk);
            return;
        }




        // 如果在等待期间发现玩家，立即切换到追逐状态
        if (parameter.target != null)
        {
           //manager.ChangeState(StateType.Chase);
        }
    }

    public void OnExit()
    {
        
    }
}


public class PlayerWalkState : IState
{
    private PlayerFSM manager;
    private PlayerParameter parameter;
    public PlayerWalkState(PlayerFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnStart()
    {
        Debug.Log("进入Walk状态");
    }
    public void OnUpdate()
    {
        #region 移动逻辑
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        float rayLengthX = 0.9f; // 略大于玩家半径
        float rayLengthY = 0.9f;
        LayerMask wallLayer = LayerMask.GetMask("Wall");

        // 分别检测X和Y方向，避免对角线同时被锁
        if (moveX != 0)
        {
            RaycastHit2D hitX = Physics2D.Raycast(manager.transform.position, Vector2.right * Mathf.Sign(moveX), rayLengthX, wallLayer);
            if (hitX.collider != null) moveX = 0;
        }
        if (moveY != 0)
        {
            RaycastHit2D hitY = Physics2D.Raycast(manager.transform.position, Vector2.up * Mathf.Sign(moveY), rayLengthY, wallLayer);
            if (hitY.collider != null) moveY = 0;
        }

        // 应用移动
        manager.transform.Translate(new Vector3(moveX, moveY, 0) * parameter.moveSpeed * Time.deltaTime, Space.World);
        // 获取鼠标在世界空间中的位置（注意：ScreenToWorldPoint 需要正确的Z值）
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(manager.transform.position).z; // 使用角色的屏幕深度
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // 计算向量（从角色指向鼠标）
        Vector2 directionMouse = mouseWorldPos - manager.transform.position;
        // 在鼠标追踪逻辑后添加：
        if (directionMouse.x > 0)
            manager.playerSpriteRenderer.flipX = false; // 朝右
        else if (directionMouse.x < 0)
            manager.playerSpriteRenderer.flipX = true;  // 朝左       
                                          // 注意：这里仅根据X轴方向决定翻转，如果鼠标在正上方，会保持上次朝向，但通常够用。
                                          // 更精细的可以结合方向角度，但先这样。

        #endregion
    }
    public void OnExit()
    {
        
    }
}

public class PlayerShootState
{
    private PlayerFSM manager;
    private PlayerParameter parameter;
    // 等待计时器
    

    public PlayerShootState(PlayerFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnStart()
    {
        Debug.Log("进入Idle状态");
        
    }
}