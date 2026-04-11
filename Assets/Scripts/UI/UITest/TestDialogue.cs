using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

//  对话面板测试
public class TestDialogue : MonoBehaviour
{
    public Transform ring;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("打开面板");
            UIManager.Instance.OpenPanel(UIManager.UIConst.Dialogue);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ring.DORotate(new Vector3(0, 0, 360), 360f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
        }
    }
}
