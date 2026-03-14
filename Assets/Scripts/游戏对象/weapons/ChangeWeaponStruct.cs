using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChangeWeaponStruct 
{
    public int lastWeaponID;
    public int currentWeaponID;
    public ChangeWeaponStruct(int lastID,int currentID ) 
    {
        lastWeaponID = lastID;
        currentWeaponID = currentID;
    }
}
