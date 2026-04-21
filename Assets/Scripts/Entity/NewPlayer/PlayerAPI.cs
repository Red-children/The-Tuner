using UnityEngine;

public class PlayerAPI : MonoBehaviour
{
    public PlayerHealth health;
    public PlayerMovement movement;
    public PlayerAttack attack;
    public PlayerDash dash;
    public PlayerWeapon weapon;

    private void Awake()
    {
        if (health == null) health = GetComponent<PlayerHealth>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (attack == null) attack = GetComponent<PlayerAttack>();
        if (dash == null) dash = GetComponent<PlayerDash>();
        if (weapon == null) weapon = GetComponent<PlayerWeapon>();
       
    }

    public void TakeDamage(int damage)
    {
        health?.TakeDamage(damage);
    }

    public void SetStunned(bool stunned)
    {
        movement?.SetStunned(stunned);
        attack.enabled = !stunned;
        dash.enabled = !stunned;
        weapon.enabled = !stunned;
    }
}