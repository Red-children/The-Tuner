using TMPro;
using UnityEngine;

public class NPCNameUI : MonoBehaviour
{
    [Header("绑定设置")]
    public Canvas targetCanvas;
    public Vector2 nameOffset = new Vector2(0, 1f);
    public int fontSize = 28;
    public Color nameColor = Color.white;

    private TextMeshProUGUI _nameText;
    private Transform _npcTransform;
    private Camera _uiCamera; 

    public void Init(Transform npcTrans, string npcName)
    {
        _npcTransform = npcTrans;
        _uiCamera = targetCanvas.worldCamera ?? Camera.main;
        CreateNameText(npcName);
    }
    private bool IsInScreen()
    {
        Vector3 vpPos = _uiCamera.WorldToViewportPoint(_npcTransform.position);
        return vpPos.z > 0 && vpPos.x is > 0 and < 1 && vpPos.y is > 0 and < 1;
    }
    private void CreateNameText(string npcName)
    {
        if (targetCanvas == null) return;

        GameObject nameObj = new GameObject($"NPC_Name_{npcName}");
        nameObj.transform.SetParent(targetCanvas.transform, false);

        _nameText = nameObj.AddComponent<TextMeshProUGUI>();
        _nameText.text = npcName;
        _nameText.fontSize = fontSize;
        _nameText.color = nameColor;
        _nameText.alignment = TextAlignmentOptions.Center;
        _nameText.raycastTarget = false;

        RectTransform rect = _nameText.rectTransform;
        rect.sizeDelta = new Vector2(200, 40);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        _nameText.transform.SetAsLastSibling();
    }
#region 对外接口
    public void SetVisible(bool isActive)
    {
        if (_nameText != null)
            _nameText.gameObject.SetActive(isActive);
    }
    public void DestroyName()
    {
        if (_nameText != null) Destroy(_nameText.gameObject);
    }
#endregion
#region 生命周期
    // 自己 LateUpdate 自己追踪
    private void LateUpdate()
    {
        if (_nameText == null || _npcTransform == null || _uiCamera == null) return;

        if(!IsInScreen())
        {
            _nameText.gameObject.SetActive(false);
            return;
        }
        _nameText.gameObject.SetActive(true);
        Vector3 worldPos = _npcTransform.position + new Vector3(nameOffset.x, nameOffset.y, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        _nameText.transform.position = screenPos;
    }
#endregion
}