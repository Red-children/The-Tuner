using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }
    //  (0, 1)
    private float globalDamageMultiplier = 0.99f;
    public float GlobalDamageMultiplier => globalDamageMultiplier;
    private float[] Multiplier = {0.99f, 0.01f};


    //  计时周期 0.5s
    private float timer = 0f;
    private float updateInterval = 0.5f;
    private float offset = 0.0f;
    //  踩点区间
    private float stepInterval = 0.05f;

    // 静态构造函数/初始化方法，自动创建物体
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreateManager()
    {
        if (Instance == null)
        {
            // 创建空物体并挂载管理器
            GameObject managerObj = new GameObject("GlobalManager");
            Instance = managerObj.AddComponent<GlobalManager>();

            DontDestroyOnLoad(managerObj);
        }
    }

    //  更新计时器
    private void UpdateTimer()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer %= updateInterval;
        }
    }

    //  倍率变化规则
    private void PublishMultiplierChangedEvent()
    {
        EventBus.Instance.Trigger(new GlobalAttackMultiplierChangedEvent
        {
            newMultiplier = globalDamageMultiplier,
            isCritical = globalDamageMultiplier > 0.5f,
            time = Time.time
        });
    }
    private void UpdateGlobalDamageMultiplier()
    {
        int index;
        if (Mathf.Abs(updateInterval - timer) < stepInterval)
        {
            index = 0;
        }
        else
        {
            index = 1;
        }
        if (globalDamageMultiplier == Multiplier[index]) 
            return;
        globalDamageMultiplier = Multiplier[index];
        PublishMultiplierChangedEvent();
    }

    void Start()
    {
        //  发布计时器上线事件
        EventBus.Instance.Trigger(new TimerOnlineEvent
        {
            time = Time.time
        });
        Debug.Log($"GlobalManager已创建(时间:{Time.time:F1}秒), 并发布了TimerOnlineEvent");
    }
    void Update()
    {
        UpdateTimer();
        UpdateGlobalDamageMultiplier();
    }

    // 防止手动创建多实例
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}