using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRuntime : MonoBehaviour
{
    [Header("运行时状态")]
    public Transform target;          // 当前目标
    public bool getHit;               // 是否受击
    public float currentHealth;       // 当前血量（初始从数据读取）

    private EnemyData data;
    public EnemyData Data => data;

    public void Init(EnemyData enemyData)
    {
        data = enemyData;
        currentHealth = data.health;
    }
}

