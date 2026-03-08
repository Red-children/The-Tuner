using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class DamageNumber : MonoBehaviour
{
    [Header("动画参数")]
    public float moveHeight = 1f;  
    public float duration = 1f;

    [Header("组件引用")]
    public Text damageText;  // 手动拖拽赋值

    void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main; // 自动获取主摄像机
        }
        if (damageText == null)
            damageText = GetComponentInChildren<Text>();
        if (damageText == null)
            Debug.LogError("DamageNumber: 没有找到 Text 组件！");
    }

    public void SetDamage(float value)
    {
        Debug.Log($"SetDamage 调用，伤害值: {value}");
        if (damageText != null)
            damageText.text = value.ToString("0");
        else
            Debug.LogError("damageText 为空，无法设置文本");

        StartAnimation();
    }

    private void StartAnimation()
    {
        Debug.Log("StartAnimation 开始");
        if (damageText == null) return;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, moveHeight, 0);

        // 确保之前动画被终止（避免冲突）
        transform.DOKill();
        damageText.DOKill();

        // 启动动画
        transform.DOMove(endPos, duration).SetEase(Ease.OutQuad);
        damageText.DOFade(0f, duration).SetEase(Ease.Linear);
        DOVirtual.DelayedCall(duration, () =>
        {
            Debug.Log("动画结束，销毁对象");
            Destroy(gameObject);
        });
    }
}