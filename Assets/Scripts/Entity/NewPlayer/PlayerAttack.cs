using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerWeapon playerWeapon;

    private void Awake()
    {
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    public void TryFire()
    {
        var weapon = playerWeapon?.GetCurrentWeapon();
        if (weapon != null)
        {
            weapon.HandleFireInput();
        }
    }
}
