using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 专门负责：查找UI、缓存引用、设置文字、图片
public class UIHPBinder : MonoBehaviour
{
    private Image _hpForegroundImage;
    private TextMeshProUGUI _hpText;

    public void InitComponents()
    {
        _hpForegroundImage = transform.Find("HPForeground").GetComponent<Image>();
        _hpText = transform.Find("HPText").GetComponent<TextMeshProUGUI>();

        if (_hpForegroundImage == null || _hpText == null)
            Debug.LogError("UIHPBinder 缺失子对象!");
    }

    // 对外接口
    public void SetHPText(string content)
    {
        if(_hpText == null) return;
        _hpText.text = content;
        _hpText.SetAllDirty();
    }

    public Image GetForegroundImage() => _hpForegroundImage;
}