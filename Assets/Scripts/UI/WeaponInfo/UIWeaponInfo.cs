using UnityEngine;

public class UIWeaponInfo : MonoBehaviour
{
    public UIWeaponInfoText text;
    private WeaponInfo currentWeapon;

    private void Update()
    {
        // 获取当前武器（通过玩家身上的 PlayerWeapon 组件）
        if (currentWeapon == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var playerWeapon = player.GetComponent<PlayerWeapon>();
                if (playerWeapon != null)
                    currentWeapon = playerWeapon.GetCurrentWeapon();
            }
        }

        if (currentWeapon != null)
        {
            text.SetDisplayText("Ammo: "+ currentWeapon.CurrentAmmo.ToString());

        }
        else
        {
            text.SetDisplayText("0");
        }
    }
}