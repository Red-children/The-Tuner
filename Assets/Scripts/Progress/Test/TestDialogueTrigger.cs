using UnityEngine;

public class TestDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] Collider2D player;
    private bool _onlyOneTime = true;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && _onlyOneTime)
        {
            dialogueTrigger.OnTriggerEnter2D(player);
            _onlyOneTime = false;
        }
            
    }
}
