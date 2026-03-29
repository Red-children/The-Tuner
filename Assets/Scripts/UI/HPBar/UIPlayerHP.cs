
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
    public UIHPText HPText;

#region Test
    void TestInit()
    {
        Init();
        _maxHP = 100f;
        _lastHP = 100f;
    }
#endregion
    void Init()
    {
        if (healthPercent == null)
        {
            healthPercent = GetComponentInChildren<UIHPForeground>();
        }
        if (HPText == null)
        {
            HPText = GetComponentInChildren<UIHPText>();
        }
        if (healthPercent == null || HPText == null)
        {
            Debug.LogError("UIPlayerHP 组件缺失!!!!!!!!!!");
        }
    }
#region 回调函数
    void OnPlayerHPChange(PlayerHealthChangedEventStruct evt)
    {
        Debug.Log("TestHP:Receive HPChangeEvent\n");
        //  初始化
        if (_maxHP - evt.maxHealth > 1e-6)
        {
            _maxHP = evt.maxHealth;
        } 
        //  更新其他信息
        _lastHP = evt.currentHealth;
        //  通知播放血条变动动画
        healthPercent.SetTargetPercent(evt.healthPercent);
        //  通知文字更新
        HPText.SetDisplayText(_lastHP + " / " + _maxHP);
    }
#endregion
#region 生命周期
    void Start()
    {
        // Init();
        TestInit();
        //  订阅血量变化事件
        EventBus.Instance.Subscribe<PlayerHealthChangedEventStruct>(OnPlayerHPChange);
    }
#endregion
}