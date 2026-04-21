using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunToneFlyingIdleState :  EnemyStateBase 
{
    private float timer;
    public RunToneFlyingIdleState(FSM fsm) : base(fsm) { }
    public override void OnStart() 
    { 
        timer = 0f; 
        //manager.animator.SetTrigger("Idle"); 
    }
    public override void OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= (data as RunToneFlyingInsectData).chargeCooldown)
            manager.ChangeState(StateType.Chase); // 这里用Chase作为预警状态
    }
}

// RunToneFlyingChaseState (实际是预警)
public class RunToneFlyingChaseState : EnemyStateBase
{
    private float timer;
    private Color originalColor;
    public RunToneFlyingChaseState(FSM fsm) : base(fsm) { }
    public override void OnStart()
    {
        timer = 0f;
        originalColor = controller.spriteRenderer.color;
        controller.spriteRenderer.color = (data as RunToneFlyingInsectData).warningColor;
        
    }
    public override void OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= (data as RunToneFlyingInsectData).warningDuration)
            manager.ChangeState(StateType.Attack);
    }
    public override void OnExit()
    {
        controller.spriteRenderer.color = originalColor;
    }
}

// RunToneFlyingChargeState

/// <summary>
/// 飞行昆虫的冲撞状态，直接向玩家位置飞行一段时间
/// </summary>

public class RunToneFlyingChargeState : EnemyStateBase
{
    private Vector2 chargeDirection;
    private float chargeSpeed;
    private RunToneFlyingInsectData insectData;
    private float chargeTime;          // 冲撞最大时间，防止无限飞行
    private float maxChargeTime = 3f;  // 3秒后自动切回Idle

    public RunToneFlyingChargeState(FSM fsm) : base(fsm) { }

    public override void OnStart()
    {
        manager.animator.SetTrigger("IsAttack");
        //转换数据类型
        insectData = data as RunToneFlyingInsectData;
        if (insectData == null)
        {
            //如果为空 直接返回Idle，避免错误
            manager.ChangeState(StateType.Idle);
            return;
        }

        //取出冲撞数据
        chargeSpeed = insectData.chargeSpeed;

        chargeTime = 0f;

        // 关键修复：目标为空时的处理
        if (runtime.target == null)
        {
            // 尝试重新获取玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                runtime.target = player.transform;
        }

        if (runtime.target != null)
        {
            chargeDirection = (runtime.target.position - manager.transform.position).normalized;
        }
        else
        {
            // 实在没有目标，随机一个方向
            chargeDirection = Random.insideUnitCircle.normalized;
            Debug.LogWarning("RunToneFlyingCharge: 没有目标，使用随机方向");
        }
    }

    public override void OnUpdate()
    {
        if (insectData == null)
        {
            manager.ChangeState(StateType.Idle);
            return;
        }

        // 执行移动
        manager.transform.Translate(chargeDirection * chargeSpeed * Time.deltaTime);

        chargeTime += Time.deltaTime;

        // 撞墙检测（可选）
        RaycastHit2D hit = Physics2D.Raycast(manager.transform.position, chargeDirection, 0.5f, LayerMask.GetMask("Wall"));
        if (hit.collider != null)
        {
            manager.ChangeState(StateType.Idle);
            return;
        }

        // 飞出太远或超时则返回Idle
        if (chargeTime >= maxChargeTime)
        {
            manager.ChangeState(StateType.Idle);
        }
    }

    public override void OnExit() { }
}