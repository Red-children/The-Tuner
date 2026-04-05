using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class ScreenFlashManager : MonoBehaviour
{
    public List<Image> flashImages;



    public float flashDuration = 0.1f;
    public Color perfectColor = Color.yellow;
    public Color greatColor = Color.cyan;
    public Color goodColor = Color.white;

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<RhythmHitEvent>(OnRhythmHit);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<RhythmHitEvent>(OnRhythmHit);
    }

    private void OnRhythmHit(RhythmHitEvent e)
    {
        // 根据判定选择颜色
        Color targetColor = goodColor;
        if (e.rank == RhythmRank.Perfect) targetColor = perfectColor;
        else if (e.rank == RhythmRank.Great) targetColor = greatColor;

        foreach (var flashImage in flashImages)
        {
            // 闪光动画：瞬间变亮然后渐隐
            flashImage.DOKill(); // 终止之前动画

            flashImage.color = targetColor;

            flashImage.DOFade(0, flashDuration).SetEase(Ease.OutQuad);
        }
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = e.intensity * 0.2f });
    }
}
