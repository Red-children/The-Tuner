using UnityEngine;

public class TestLoading : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("打开面板");
            UIManager.Instance.OpenPanel(UIManager.UIConst.Loading);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            UIManager.Instance.ClosePanel(UIManager.UIConst.Loading);
        }
    }
}