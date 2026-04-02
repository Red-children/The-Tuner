using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBattle : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.OpenPanel(UIManager.UIConst.Battle);
        UIManager.Instance.OpenPanel(UIManager.UIConst.Crosshair);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            UIManager.Instance.ShowPanel(UIManager.UIConst.Battle);
            UIManager.Instance.ShowPanel(UIManager.UIConst.Crosshair);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            UIManager.Instance.HidePanel(UIManager.UIConst.Battle);
        }
    }
}
