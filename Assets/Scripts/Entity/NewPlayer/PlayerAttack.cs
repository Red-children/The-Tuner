using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerWeapon playerWeapon;

    private void Awake()
    {
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    private void Update()
    {
        HandleShootInput();
        HandleMeleeInput();
    }

    private void HandleShootInput()
    {
        if (playerWeapon == null) return;

        WeaponInfo currentWeapon = playerWeapon.GetCurrentWeapon();
        if (currentWeapon == null) return;

        if (currentWeapon is BassCannon bassCannon)
        {
            if (Input.GetMouseButtonDown(0))
            {
                bassCannon.StartCharge();
            }
            if (Input.GetMouseButtonUp(0))
            {
                bassCannon.ReleaseCharge();
            }
        }
        else if (currentWeapon is StandardFirearm)
        {
            if (Input.GetMouseButton(0))
            {
                currentWeapon.HandleFireInput();
            }
        }
    }

    private void HandleMeleeInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            // 近战攻击逻辑
            // 这里可以触发近战动画事件
        }
    }
}
