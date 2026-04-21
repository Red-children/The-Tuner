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
    public float moveHeight = 1f;
    public float duration = 1f;

    [Header("判定等级颜色配置")]
    public Color perfectColor = new Color(1f, 0.8f, 0f);      // 金色
    public Color greatColor = new Color(0.3f, 0.8f, 1f);     // 亮蓝
    public Color goodColor = Color.white;                    // 白色
    public Color missColor = new Color(0.5f, 0.5f, 0.5f);    // 灰色

    [Header("字号变化")]
    public float perfectScale = 1.5f;
    public float greatScale = 1.2f;
    public float goodScale = 1.0f;

    public TextMeshProUGUI damageText;

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

  
    // 设置伤害数字并开始动画
  public void SetDamage(float value, RhythmRank rank = RhythmRank.Good)
    {
        if (damageText == null) return;
        damageText.text = value.ToString("0");

        // 根据等级设置颜色和字号
        switch (rank)
        {
            case RhythmRank.Perfect:
                damageText.color = perfectColor;
                damageText.fontSize = Mathf.RoundToInt(damageText.fontSize * perfectScale);
                break;
            case RhythmRank.Great:
                damageText.color = greatColor;
                damageText.fontSize = Mathf.RoundToInt(damageText.fontSize * greatScale);
                break;
            case RhythmRank.Good:
                damageText.color = goodColor;
                break;
            default:
                damageText.color = missColor;
                break;
        }

        // 可选：Perfect 时加一个简单的放大动画
        if (rank == RhythmRank.Perfect)
        {
            damageText.transform.localScale = Vector3.one * 1.3f;
            damageText.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        }

        StartAnimation();
    }


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
    

}