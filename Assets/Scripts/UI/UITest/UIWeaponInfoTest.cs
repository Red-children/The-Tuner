using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  触发武器切换事件的测试
public class UIWeaponInfoTest : MonoBehaviour
{
    public ChangeWeaponStruct evt;
    void Start()
    {
        evt.currentWeaponID = 1;
        evt.lastWeaponID = 1;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            evt.currentWeaponID = 1;
            if (evt.currentWeaponID == evt.lastWeaponID)
            {
                return;
            }
            EventBus.Instance.Trigger<ChangeWeaponStruct>(evt);
            evt.lastWeaponID = evt.currentWeaponID;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            evt.currentWeaponID = 2;
            if (evt.currentWeaponID == evt.lastWeaponID)
            {
                return;
            }
            EventBus.Instance.Trigger<ChangeWeaponStruct>(evt);
            evt.lastWeaponID = evt.currentWeaponID;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            evt.currentWeaponID = 3;
            if (evt.currentWeaponID == evt.lastWeaponID)
            {
                return;
            }
            EventBus.Instance.Trigger<ChangeWeaponStruct>(evt);
            evt.lastWeaponID = evt.currentWeaponID;
        }
    }
}
