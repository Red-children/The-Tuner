using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerWeapon playerWeapon;
    private PlayerAnimation playerAnimation;
    private PlayerLegController playerLeg;
    private PlayerArmController playerArm;
    private PlayerIdleArmController idleArm;

    private void Awake()
    {
        playerWeapon = GetComponent<PlayerWeapon>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerLeg = GetComponent<PlayerLegController>();
        playerArm = GetComponent<PlayerArmController>();
        idleArm = GetComponent<PlayerIdleArmController>();
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
                playerAnimation?.PlayBodyAttack();
                playerArm?.PlayArmAttack();
                idleArm?.PlayArmAttack();
            }
            if (Input.GetMouseButtonUp(0))
            {
                bassCannon.ReleaseCharge();
            }
        }
        else if (currentWeapon is StandardFirearm)
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentWeapon.HandleFireInput();
                playerAnimation?.PlayBodyAttack();
                playerArm?.PlayArmAttack();
                idleArm?.PlayArmAttack();
            }
        }
    }

    private void HandleMeleeInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            playerAnimation?.PlayBodyAttack();
            playerArm?.PlayArmAttack();
            idleArm?.PlayArmAttack();
            playerLeg?.PlayLegAttack();
        }
    }
}
