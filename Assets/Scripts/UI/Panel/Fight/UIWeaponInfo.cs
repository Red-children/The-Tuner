using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIWeaponInfo : MonoBehaviour
{
    [System.Serializable]
    struct WeaponPanel
    {
        public Image background;
        public Image[] weapons;
    }
    
    [SerializeField] private WeaponPanel weaponInfo;
    
    private Image _currentHighlightWeapon;
    private int _currentWeaponIndex = -1;
    
    #region 初始化
    void Awake()
    {
        EventBus.Instance.Subscribe<WeaponChangedEvent>(OnWeaponChange);
        EventBus.Instance.Subscribe<AmmoChangedEvent>(OnAmmoChange);
    }
    
    void Start()
    {
        foreach (var weapon in weaponInfo.weapons)
        {
            if (weapon != null)
            {
                weapon.fillAmount = 0f;
                weapon.transform.localScale = Vector3.one;
            }
        }
    }
    #endregion
    
    #region 回调函数
    void OnWeaponChange(WeaponChangedEvent evt)
    {
        _currentWeaponIndex = evt.weaponId;
        HighlightWeapon(_currentWeaponIndex);
    }
    
    void OnAmmoChange(AmmoChangedEvent evt)
    {
        // 弹药变化统一走这里：消耗弹药、装填、捡取弹药等都触发此事件
        UpdateWeaponFillAmount(evt.weaponId, evt.currentAmmo, evt.reserveAmmo);
    }
    #endregion
    
    #region 武器高亮
    void HighlightWeapon(int index)
    {
        if (index < 0 || index >= weaponInfo.weapons.Length) return;
        
        Image targetWeapon = weaponInfo.weapons[index];
        if (targetWeapon == null) return;
        
        // 恢复上一个武器缩放
        if (_currentHighlightWeapon != null && _currentHighlightWeapon != targetWeapon)
        {
            _currentHighlightWeapon.transform.DOScale(1f, 0.2f);
        }
        
        // 放大新武器
        _currentHighlightWeapon = targetWeapon;
        targetWeapon.transform.DOScale(1.2f, 0.2f);
    }
    #endregion
    
    #region 弹药填充动画（统一入口）
    void UpdateWeaponFillAmount(int weaponIndex, int currentAmmo, int maxAmmo)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponInfo.weapons.Length) return;
        
        Image weapon = weaponInfo.weapons[weaponIndex];
        if (weapon == null) return;
        
        float targetFill = maxAmmo > 0 ? (float)currentAmmo / maxAmmo : 0f;
        targetFill = Mathf.Clamp01(targetFill);
        
        // 统一使用同一个动画
        weapon.DOFillAmount(targetFill, 0.25f).SetEase(Ease.OutCubic);
        
        // 低弹药警告（可选）
        if (currentAmmo <= maxAmmo * 0.2f && currentAmmo > 0)
        {
            Color originalColor = weapon.color;
            weapon.DOColor(Color.red, 0.1f)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() => weapon.DOColor(originalColor, 0.1f));
        }
    }
    #endregion
    
    #region 生命周期
    void OnDestroy()
    {
        foreach (var weapon in weaponInfo.weapons)
        {
            if (weapon != null)
            {
                weapon.DOKill();
                weapon.transform.DOKill();
            }
        }
    }
    #endregion
}