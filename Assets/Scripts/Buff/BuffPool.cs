using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// BuffPool 负责管理所有的 BuffData，并提供随机获取 Buff 的功能

public class BuffPool : MonoBehaviour
{
    public static BuffPool Instance;
    public List<BuffData> allBuffs;   // 直接在 Inspector 中拖入所有 BuffData

    private void Awake()
    {
        Instance = this;
    }

    public List<BuffData> GetRandomBuffs(int count)
    {

        if (allBuffs == null || allBuffs.Count == 0) return new List<BuffData>();

        // 随机排序并取前 count 个
        List<BuffData> shuffled = new List<BuffData>(allBuffs);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int rand = Random.Range(i, shuffled.Count);
            var temp = shuffled[i];
            shuffled[i] = shuffled[rand];
            shuffled[rand] = temp;
        }

        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
    }
}