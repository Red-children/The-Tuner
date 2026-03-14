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

public struct PlayHealthChangedEventStruct
{
    public float currentHealth;     //  变化后血量  
    public float maxHealth;         //  血量最大值  
    public float healthPercent => currentHealth / maxHealth;
}