using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

//  武器UI主控
public class UIWeaponInfo : MonoBehaviour
{
    //  备弹
    private int _reserveAmmo;
    //  载弹
    private int _capacityAmmo;
    //  武器快捷键键位
    private int _number;

    public UIWeaponInfoText text;
    //  Test
    private WeaponInfo weapon;

    //  获取玩家武器信息
    private void SearchWeaponInfo()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("UIWeaponInfoTest:Weapon Not Found.");
            return;
        }
        weapon = player.GetComponentInChildren<WeaponInfo>();
    }
    private void Init()
    {
        _number = 1;
        _capacityAmmo = 0;
        _reserveAmmo = 0;
        if (text == null)
        {
            text = GetComponentInChildren<UIWeaponInfoText>();
        }

        if (text == null)
        {
            Debug.Log("UIWeaponInfo 组件缺失!!!!");
            return;
        }
        //  获取玩家武器信息
        SearchWeaponInfo();
        
        EventBus.Instance.Subscribe<ChangeWeaponStruct>(OnChangeWeapon);
        // EventBus.Instance.Subscribe<ChangeAmmoCapEvent>(OnChangeAmmo);
    }

    private void UpdateInfo()
    {
        _capacityAmmo = weapon.GetCurrentAmmo();
        text.SetDisplayText("Ammo: " + _capacityAmmo + " / " + _reserveAmmo);
    }
#region 回调函数
    void OnChangeWeapon(ChangeWeaponStruct evt)
    {
        
    }
    // void OnChangeAmmo(ChangeAmmoCapEvent evt)
    // {
    //     _capacityAmmo = weapon.GetCurrentAmmo();
    //     text.SetDisplayText("Ammo: " + _capacityAmmo + " / " + _reserveAmmo);
    // }
    #endregion
    #region 生命周期
    void Start()
    {
        Init();
    }
    void Update()
    {
        UpdateInfo();
    }
#endregion
#region Test
#endregion
}
