using UnityEngine;

public class BossSkill : MonoBehaviour
{
    public virtual void ExecuteSkill()
    {
        Debug.Log($"[{gameObject.name}] Boss技能释放！");
    }
}
