using UnityEngine;
using System.Collections.Generic;

public class PlayerWeapon : MonoBehaviour
{
    [Header("武器信息 ")]
    public WeaponInfo[] weapons;       //玩家持有的武器列表，初始可以在编辑器里设置，也可以通过捡起武器动态添加
    public WeaponInfo currentWeapon;      //当前武器

    private int currentIndex = 0;

    private void Start()
    {
        //读取初始武器信息，如果编辑器里没有设置，则尝试从子物体获取
        if (weapons == null || weapons.Length == 0)
            weapons = GetComponentsInChildren<WeaponInfo>();

        if (weapons.Length > 0)
        {
            currentWeapon = weapons[0];
            // 切换到初始武器，确保只有当前武器激活
            foreach (var w in weapons) w.gameObject.SetActive(false);
            currentWeapon.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // ���ּ��л�
        for (int i = 0; i < weapons.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchToWeapon(i);
                break;
            }
        }

        // �����л�
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && weapons.Length > 0)
        {
            int delta = scroll > 0 ? 1 : -1;
            int newIndex = (currentIndex + delta + weapons.Length) % weapons.Length;
            SwitchToWeapon(newIndex);
        }

        // ����ָ�����

        Vector2 directionMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition) - currentWeapon.transform.position;

        if (currentWeapon != null)
        {
            Vector2 weaponDir = directionMouse; // ��������ҵ����һ��
            float weaponAngle = Mathf.Atan2(weaponDir.y, weaponDir.x) * Mathf.Rad2Deg;
            currentWeapon.transform.rotation = Quaternion.Euler(0, 0, weaponAngle);
        }

    }

    /// <summary>
    /// 传入要切换的武器索引，进行切换
    /// </summary>
    /// <param name="index"></param>
    private void SwitchToWeapon(int index)
    {
        if (index == currentIndex) return;
        if (index < 0 || index >= weapons.Length) return;

        
        currentWeapon.gameObject.SetActive(false);
        currentWeapon = weapons[index];
        currentWeapon.gameObject.SetActive(true);
        currentIndex = index;

        // ���������л��¼����� UI ��ģ�����
        EventBus.Instance.Trigger(new WeaponChangedEvent
        {
            newWeapon = currentWeapon,
        });
    }

    /// <summary>
    /// 得到目前的武器接口
    /// </summary>
    /// <returns></returns>
    public WeaponInfo GetCurrentWeapon()
    {
        return currentWeapon;
    }

    /// <summary>
    /// 捡起武器接口，返回是否成功捡起（比如已经有同类型武器了）
    /// </summary>
    /// <param name="weaponBase"></param>
    /// <param name="type"></param>
    /// <returns></returns>
  public bool PickupWeapon(WeaponInfo newWeaponInfo)
{
    if (newWeaponInfo == null) return false;

    GameObject newWeaponObj = newWeaponInfo.gameObject;
    Debug.Log($"[Pickup] Start: {newWeaponObj.name}");

    // 1. 先转移所有权，但暂不设置父级（避免在替换时丢失）
    // 等确定替换/添加后再设置父级

    // 2. 检查是否已有同类型武器
    for (int i = 0; i < weapons.Length; i++)
    {
        if (weapons[i] != null && weapons[i].weaponType == newWeaponInfo.weaponType)
        {
            Debug.Log($"[Pickup] Replacing at index {i}");

            // 保存旧武器GameObject
            GameObject oldWeaponObj = weapons[i].gameObject;

            // 将新武器设为玩家子物体
            newWeaponObj.transform.SetParent(transform);
            newWeaponObj.transform.localPosition = Vector3.zero;
            newWeaponObj.transform.localRotation = Quaternion.identity;
            newWeaponObj.SetActive(true);

            // 替换数组元素
            weapons[i] = newWeaponInfo;

            // 销毁旧武器（确保不是当前武器？）
            if (oldWeaponObj != null)
            {
                // 如果旧武器是当前武器，需要先切换到新武器再销毁
                if (currentWeapon != null && currentWeapon.gameObject == oldWeaponObj)
                {
                    currentWeapon = newWeaponInfo;
                }
                Destroy(oldWeaponObj);
                Debug.Log($"[Pickup] Destroyed old weapon: {oldWeaponObj.name}");
            }

            SwitchToWeapon(i);
            return true;
        }
    }

    // 3. 新增武器
    Debug.Log("[Pickup] Adding new weapon");

    newWeaponObj.transform.SetParent(transform);
    newWeaponObj.transform.localPosition = Vector3.zero;
    newWeaponObj.transform.localRotation = Quaternion.identity;
    newWeaponObj.SetActive(false); // 先隐藏，切换时激活

    // 过滤掉空引用，构建新数组
    var list = new List<WeaponInfo>();
    foreach (var w in weapons)
        if (w != null) list.Add(w);
    list.Add(newWeaponInfo);
    weapons = list.ToArray();

    int newIndex = weapons.Length - 1;
    SwitchToWeapon(newIndex);
    Debug.Log($"[Pickup] Added at index {newIndex}, total weapons: {weapons.Length}");
    return true;
}
}


public struct WeaponChangedEvent
{
    public WeaponInfo newWeapon;
    public int weaponId;
}
