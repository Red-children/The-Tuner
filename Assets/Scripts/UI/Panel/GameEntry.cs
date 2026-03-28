using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  进入游戏加载主界面
public class GameEntry : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.OpenPanel(UIManager.UIConst.MainMenu);
    }
}
