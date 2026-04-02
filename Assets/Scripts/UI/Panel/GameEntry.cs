using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  进入游戏加载主界面
public class GameEntry : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
            UIManager.Instance.OpenPanel(UIManager.UIConst.MainMenu);
    }
}
