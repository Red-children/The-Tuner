using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBattle : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.D))
        // {
        //     UIManager.Instance.OpenPanel(UIManager.UIConst.Battle);
        //     UIManager.Instance.OpenPanel(UIManager.UIConst.Crosshair);
        // }
        // else if (Input.GetKeyDown(KeyCode.F))
        // {
        //     UIManager.Instance.ClosePanel(UIManager.UIConst.Battle);
        // }
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse Down");
            EventBus.Instance.Trigger<EnemyHitEvent>(new(1, RhythmRank.Good));
        } 
        else if (Input.GetKeyDown(KeyCode.F))
        {
            UIManager.Instance.OpenPanel(UIManager.UIConst.Crosshair);            
        }
    }
}
