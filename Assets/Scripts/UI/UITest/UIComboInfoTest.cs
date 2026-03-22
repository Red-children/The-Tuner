using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComboInfoTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            EventBus.Instance.Trigger<PlayerAtkEvent>(new PlayerAtkEvent());
            EventBus.Instance.Trigger<EnemyHitEvent>(new EnemyHitEvent(1));
        }
    }
}
