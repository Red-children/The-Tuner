
using UnityEngine;
using UnityEngine.UI;
public class UIPlayerHP : MonoBehaviour
{
    //  玩家最大生命值
    private float _maxHP = 0;
    //  玩家上一次生命值变化时的生命值
    private float _lastHP = 0;
    [Header("剩余生命值条")]
    public UIHPForeground healthPercent;

    void Init()
    {
        if (healthPercent == null)
        {
            healthPercent = GetComponentInChildren<UIHPForeground>();
        }
        if (healthPercent == null)
        {
            Debug.LogError("未找到 血条组件 UIHPForeground");
        }
    }
#region 回调函数
    void OnPlayerHPChange(PlayHealthChangedEventStruct evt)
    {
        //  初始化
        if (_maxHP - evt.maxHealth > 1e-6)
        {
            _maxHP = evt.maxHealth;
        } 
        //  更新其他信息
        _lastHP = evt.currentHealth;
        //  播放血条变动动画
        healthPercent.SetTargetPercent(evt.healthPercent);
    }
#endregion
#region 生命周期
    void Start()
    {
        //  订阅血量变化事件
        EventBus.Instance.Subscribe<PlayHealthChangedEventStruct>(OnPlayerHPChange);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
#endregion
}