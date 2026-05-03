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
        if(Input.GetKeyDown(KeyCode.D))
        {
            UIManager.Instance.OpenPanel(UIManager.UIConst.Battle);
            UIManager.Instance.OpenPanel(UIManager.UIConst.Crosshair);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            UIManager.Instance.ClosePanel(UIManager.UIConst.Battle);
        }
    }
}
