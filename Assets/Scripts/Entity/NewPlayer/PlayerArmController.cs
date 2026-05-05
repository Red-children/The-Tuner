using UnityEngine;

public class PlayerArmController : MonoBehaviour
{
    [Header("组件引用")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("闲置手臂引用（用于同步翻转）")]
    public PlayerIdleArmController idleArm;

    private Camera mainCamera;
    private float currentAngle = 0f; // 记录当前旋转角

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    // 由PlayerWeapon每帧调用，传入统一的旋转角度
   public void ApplyAiming(Vector3 weaponPosition)
{
    Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    Vector3 armPosition = transform.position;

    // 计算从手臂到鼠标的方向
    Vector2 direction = mouseWorldPos - armPosition;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    // 让手臂跟着武器旋转，不区分左右，一次性到位
    transform.rotation = Quaternion.Euler(0, 0, angle+180);

    // 判断Y轴翻转：鼠标在右则翻转Y
    bool isMouseRight = mouseWorldPos.x > armPosition.x;
    Vector3 scale = transform.localScale;
    scale.y = isMouseRight ? -1 : 1;
    transform.localScale = scale;

    // 闲置手臂同步（它只处理自己的翻转）
    if (idleArm != null)
    {
        idleArm.SetFlipX(isMouseRight); // 闲置手臂单独处理X翻转
    }
}
    // 以下是你原来的动画事件回调，保留不变
    public void PlayArmAttack()
    {
        if (animator != null)
            animator.SetTrigger("ArmAttack");
    }

    public void OnArmAttackAnimationEnd()
    {
        if (animator != null)
            animator.ResetTrigger("ArmAttack");
    }
}