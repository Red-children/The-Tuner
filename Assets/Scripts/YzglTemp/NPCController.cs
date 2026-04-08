using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("NPC基本信息")]
    [SerializeField] private string npcName = "NPC01";
    [SerializeField] private bool startEnabled = true;

    [Header("模块引用")]
    [SerializeField] private NPCCommunication communication;

    [Header("名字UI")]
    [SerializeField] private NPCNameUI nameUI;

    private bool _isEnabled;
    
    private void Awake()
    {
        if (communication == null)
            communication = GetComponent<NPCCommunication>();

        _isEnabled = startEnabled;

        if (nameUI != null)
            nameUI.Init(transform, npcName);

        if (_isEnabled) EnableCommunication();
        else DisableCommunication();
    }

    public void EnableCommunication()
    {
        if (communication != null)
        {
            communication.enabled = true;
            communication.EnableCommunication();
        }
        _isEnabled = true;

        if (nameUI != null)
            nameUI.SetVisible(true);
    }

    public void DisableCommunication()
    {
        if (communication != null)
        {
            communication.enabled = false;
            communication.DisableCommunication();
        }
        _isEnabled = false;

        if (nameUI != null)
            nameUI.SetVisible(false);
    }

    private void OnDestroy()
    {
        if (nameUI != null)
            nameUI.DestroyName();
    }
}