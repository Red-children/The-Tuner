using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
public class UIHPForeground : MonoBehaviour
{
    public Image image;
    [Header("血条动画参数")]
    public float smoothTime = 0.2f; // 平滑时间（0.2秒完成缓动）
    
    // private float _targetHealth = 1f; // 目标血量（0~1）
    public float _targetHealth = 1f; // 目标血量（0~1）
    // private float _currentDisplayHealth = 0f; // 当前显示血量
    public float _currentDisplayHealth = 0f; // 当前显示血量
    private float _velocity; // 关键：速度变量（成员变量+初始0）

    // void Test()
    // {
    //     if(Input.GetMouseButtonDown(0))
    //     {
    //         _targetHealth -= 0.1f;
    //         _targetHealth = Mathf.Clamp01(_targetHealth);
    //         Debug.Log("Left Mouse Button Down!!!!!!!" + 
    //         $"Current _targetHealth:{_targetHealth}");
    //     }
    // }
    void Init()
    {
        if(image == null)
        {
            image = GetComponent<Image>();
        }
        if (image == null)
        {
            Debug.LogError("UIHPForeground 未找到组件!!!!");
            return;
        }
    }
    bool IsHPChanged()
    {
        return Mathf.Abs(_targetHealth - _currentDisplayHealth) > 1e-6;
    }
    void HPChangeAnimation()
    {
        if (IsHPChanged())
        //  带阻尼的缓动
        _currentDisplayHealth = Mathf.SmoothDamp(
            _currentDisplayHealth, // 当前值
            _targetHealth,         // 目标值
            ref _velocity,         // 速度变量
            smoothTime             // 平滑时间
            // maxSpeed和deltaTime用默认值即可
        );
        
        // 更新血条填充量
        image.fillAmount = _currentDisplayHealth;
    }
#region 生命周期
    void Start()
    {
        Init();
    }
    void Update()
    {
        HPChangeAnimation();
    }
#endregion

#region 对外接口
    public void SetTargetPercent(float percent)
    {
        _targetHealth = Mathf.Clamp01(percent);
    }
#endregion

}
