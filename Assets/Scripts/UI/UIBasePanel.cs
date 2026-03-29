using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBasePanel : MonoBehaviour
{
    private bool _isRemoved = false;
    private string _name;

    public virtual void OpenPanel(string name)
    {
        this.name = name;
        gameObject.SetActive(true);
    }
    public virtual void ClosePanel()
    {
        _isRemoved = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
