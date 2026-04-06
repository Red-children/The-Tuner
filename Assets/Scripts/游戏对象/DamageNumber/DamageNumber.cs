using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 伤害数字类，处理伤害数字的显示和动画效果
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [Header("动画参数")]
    public float moveHeight = 1f;  // 伤害数字上升的高度
    public float duration = 1f;    // 动画持续时间

    [Header("伤害数字组件")]
    public TextMeshProUGUI  damageText;  // 伤害数字的文本组件

    void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();  //得到Canvas组件
         // 确保Canvas的worldCamera已设置
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main; // 将Canvas的worldCamera设置为主摄像机，确保UI元素正确渲染
        }
        if (damageText == null)
            damageText = GetComponentInChildren<TextMeshProUGUI>();
        if (damageText == null)
            Debug.LogError("DamageNumber: 未找到 TextMeshProUGUI 组件");
    }

    #region 设置伤害数字和开始动画
    // 设置伤害数字并开始动画
    public void SetDamage(float value)
    {
        Debug.Log($"SetDamage ֵ: {value}");
        if (damageText != null)
            damageText.text = value.ToString("0");
        else
            Debug.LogError("damageText 缺失，无法显示伤害数字");

        StartAnimation();
    }
    #endregion

    #region 伤害数字动画，包含上升和淡出效果，并在动画结束后销毁对象
    private void StartAnimation()
    {
        Debug.Log("伤害数字动画开始");
        if (damageText == null) return;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, moveHeight, 0);

        // 先杀死之前的动画，避免动画叠加导致位置和透明度异常
        transform.DOKill();
        damageText.DOKill();

        // 使用DOTween创建上升和淡出动画
        //位置动画：从当前位置上升到目标位置 ，使用OutQuad缓动函数实现自然的上升效果
        transform.DOMove(endPos, duration).SetEase(Ease.OutQuad);

        // 透明度动画：从完全不透明逐渐变为完全透明，使用Linear缓动函数实现匀速淡出效果
        damageText.DOFade(0f, duration).SetEase(Ease.Linear);
        DOVirtual.DelayedCall(duration, () =>
        {
            Debug.Log("伤害数字动画结束");
            Destroy(gameObject);
        });
    }
    #endregion

}