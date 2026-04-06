using UnityEngine;

public class RoomData : MonoBehaviour
{
    [Header("门的方向（四选一或多选）")]
    public bool hasUpDoor;
    public bool hasDownDoor;
    public bool hasLeftDoor;
    public bool hasRightDoor;

    // 方便转换为数组
    public bool[] GetDoorDirections()
    {
        return new bool[] { hasUpDoor, hasDownDoor, hasLeftDoor, hasRightDoor };
    }
}