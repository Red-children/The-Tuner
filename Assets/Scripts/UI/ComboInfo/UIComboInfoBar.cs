using UnityEngine;
using UnityEngine.UI;

public class UIComboInfoBar : MonoBehaviour
{
    public Image image;

    //  动画参数
    [Header("动画参数")]
    public float duration = 1f;
    //  动画运动内部变量
    private float _currentTime = 0;
    private bool _isRunning = true;

    #region 初始化
    void Init()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        if (image == null)
        {
            Debug.LogError("UIComboInfo 未找到组件!!!!");
            return;
        }
        _currentTime = duration;
    }
    #endregion

    void CoolDownAnimation()
    {
        if (_isRunning)
        {
            _currentTime += Time.deltaTime;
            //  插值比例
            float t = Mathf.Clamp01(_currentTime / duration);
            image.fillAmount = Mathf.Lerp(1f, 0f, t);
            if (t >= 1f)
            {
                image.fillAmount = 0f;
            }
        }
    }
#region 生命周期
    void Start()
    {
        Init();
    }
    void Update()
    {
        CoolDownAnimation();
    }
#endregion

#region 对外接口
    public void StartOrResetCoolDown()
    {
        _isRunning = true;
        image.gameObject.SetActive(_isRunning);
        _currentTime = 0f;
    }
    public void StopCoolDown()
    {
        _isRunning = false;
        image.gameObject.SetActive(_isRunning);
        _currentTime = duration;
    }
#endregion
}
