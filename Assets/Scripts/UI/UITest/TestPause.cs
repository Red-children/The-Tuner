using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPause : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("打开面板");
            UIManager.Instance.OpenPanel(UIManager.UIConst.Pause);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            UIManager.Instance.ClosePanel(UIManager.UIConst.Pause);
        }
    }
}
