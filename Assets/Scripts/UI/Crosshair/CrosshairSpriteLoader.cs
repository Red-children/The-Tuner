using UnityEngine;
using UnityEngine.UI;

// 准星图片加载脚本（挂载在UICrosshair对象上）
public class CrosshairSpriteLoader : MonoBehaviour
{
    [Header("准星组件（自动查找）")]
    public Image _crosshairSmall; // 小圆准星
    public Image _crosshairBig;   // 大同心圆

    // 图片路径常量（Resources目录下，无需后缀）
    private const string SmallCirclePath = "PicUI/CircleSmall";
    private const string BigCirclePath = "PicUI/CircleBig";

    // 对外提供组件引用（给动画脚本用）
    public Image CrosshairSmall => _crosshairSmall;
    public Image CrosshairBig => _crosshairBig;

    // 初始化准星图片（由主控脚本调用）
    public void InitCrosshairSprites()
    {
        Debug.Log("CrosshairSpriteLoader: 初始化准星图片...");

        // 1. 查找准星组件（自动查找子对象）
        FindCrosshairComponents();

        // 2. 加载图片
        LoadBigCircleSprite();
        LoadSmallCircleSprite();

        // 3. 初始化显示状态
        InitCrosshairState();
    }

    #region 内部逻辑
    // 自动查找准星子组件
    private void FindCrosshairComponents()
    {
        _crosshairSmall = transform.Find("CrosshairSmall")?.GetComponent<Image>();
        _crosshairBig = transform.Find("CrosshairBig")?.GetComponent<Image>();

        if (_crosshairSmall == null)
        {
            Debug.LogError("CrosshairSpriteLoader: 未找到CrosshairSmall子对象！");
        }
        if (_crosshairBig == null)
        {
            Debug.LogError("CrosshairSpriteLoader: 未找到CrosshairBig子对象！");
        }
    }

    // 加载大圆环图片
    private void LoadBigCircleSprite()
    {
        if (_crosshairBig == null) return;

        Sprite bigCircleSprite = Resources.Load<Sprite>(BigCirclePath);
        if (bigCircleSprite == null)
        {
            Debug.LogError($"CrosshairSpriteLoader: 未找到图片 {BigCirclePath}");
            return;
        }

        _crosshairBig.sprite = bigCircleSprite;
        _crosshairBig.type = Image.Type.Simple;
        _crosshairBig.preserveAspect = true;
        Debug.Log("CrosshairSpriteLoader: 大圆环图片加载成功！");
    }

    // 加载小圆准星图片
    private void LoadSmallCircleSprite()
    {
        if (_crosshairSmall == null) return;

        Sprite smallCircleSprite = Resources.Load<Sprite>(SmallCirclePath);
        if (smallCircleSprite == null)
        {
            Debug.LogError($"CrosshairSpriteLoader: 未找到图片 {SmallCirclePath}");
            return;
        }

        _crosshairSmall.sprite = smallCircleSprite;
        _crosshairSmall.type = Image.Type.Simple;
        _crosshairSmall.preserveAspect = true;
        Debug.Log("CrosshairSpriteLoader: 小圆准星图片加载成功！");
    }

    // 初始化准星显示状态
    private void InitCrosshairState()
    {
        if (_crosshairSmall != null) _crosshairSmall.enabled = true;
        if (_crosshairBig != null) _crosshairBig.enabled = true;
    }
    #endregion
}