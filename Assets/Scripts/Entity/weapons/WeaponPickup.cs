using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponInfo weaponInfo;

    private void Awake()
    {
        if (weaponInfo == null) weaponInfo = GetComponent<WeaponInfo>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerWeapon pw = other.GetComponent<PlayerWeapon>();
        if (pw != null && pw.PickupWeapon(weaponInfo))
        {
            // 转移成功后，移除拾取脚本和碰撞体，但保留GameObject激活状态
            Destroy(this);
            Collider2D col = GetComponent<Collider2D>();
            if (col) Destroy(col);
            Debug.Log("[WeaponPickup] Pickup success, weapon transferred to player.");
        }
    }
}