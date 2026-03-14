using UnityEngine;
using System.Collections.Generic;

public enum DstrObjType
{
    WoodBox,
    EnemyBox,
    NPCBox,
}

[System.Serializable]
public class DstrObjStats
{
    public DstrObjType objType;
    public string objName;

    public float maxHealth = 30f;

    public Sprite objectSprite;
    public GameObject destroyEffect;
    public GameObject spawnUnit;
}

[CreateAssetMenu(fileName = "DstrObjBase", menuName = "Environment/DstrObjBase")]
public class DstrObjBase : ScriptableObject
{
    public List<DstrObjStats> objList;

    public DstrObjStats GetObjStats(DstrObjType type)
    {
        foreach (var obj in objList)
        {
            if (obj.objType == type)
                return obj;
        }

        return null;
    }
}