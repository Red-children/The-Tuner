using Unity.VisualScripting;

public struct ChangeWeaponStruct
{
    public int lastWeaponID;
    public int currentWeaponID;
    public ChangeWeaponStruct(int lastID, int currentID)
    {
        lastWeaponID = lastID;
        currentWeaponID = currentID;
    }
}

public struct PlayerHealthChangedEventStruct
{
    public float currentHealth;     //  变化后血量  
    public float maxHealth;         //  血量最大值  
    public float healthPercent => currentHealth / maxHealth;
}

//  TODO:
public struct ChangeAmmoCapEvent
{
    public int currentAmmo; //  Capacity Before shooting
    public int nextAmmo;    //  Capacity After shooting
    public int reserveAmmo; //  备弹
    public int weaponId;    //  
}

public struct PlayerReloadEvent
{
    public float duration;  //  换弹持续时间()
}