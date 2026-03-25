using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffPool : MonoBehaviour
{
    public static BuffPool Instance;
    public List<BuffData> allBuffs;

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
