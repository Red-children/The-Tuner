using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMainMenu : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            UIManager.Instance.OpenPanel(UIManager.UIConst.MainMenu);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            UIManager.Instance.ClosePanel(UIManager.UIConst.MainMenu);
        }
    }
}
