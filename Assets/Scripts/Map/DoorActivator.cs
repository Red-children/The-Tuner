using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorActivator : MonoBehaviour
{
    [Header("计数配置")]
    [SerializeField] private int requiredHits = 5;
    [SerializeField] private int perfectReduction = 3;
    [SerializeField] private int greatReduction = 2;
    [SerializeField] private int goodReduction = 1;

    [Header("关联的门")]
    [SerializeField] private Door targetDoor;

    private int currentCount;

    private void Awake()
    {
        if (targetDoor == null)
            targetDoor = GetComponentInParent<Door>();
        currentCount = requiredHits;
    }

    /// <summary>
    /// 被子弹命中时调用
    /// </summary>
    /// <param name="rank">子弹携带的节奏判定等级</param>
    public void TakeHit(RhythmRank rank)
    {
        int reduction = GetReductionByRank(rank);
        currentCount -= reduction;
        Debug.Log($"装置被命中，判定：{rank}，减少 {reduction}，剩余 {currentCount}");

        if (currentCount <= 0)
        {
            targetDoor.Open();
            gameObject.SetActive(false); // 装置失效
        }
    }

    private int GetReductionByRank(RhythmRank rank)
    {
        return rank switch
        {
            RhythmRank.Perfect => perfectReduction,
            RhythmRank.Great => greatReduction,
            RhythmRank.Good => goodReduction,
            _ => 0
        };
    }
}