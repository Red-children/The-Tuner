using UnityEngine;
using System.Collections.Generic;

public class PlayerWeapon : MonoBehaviour
{
    [Header("�����б�����ѡ�����������Զ���ȡ��")]
    public WeaponInfo[] weapons;          // ���ֶ���ק�������� ���Ŀǰ���е������б�����������û��Զ������������ϵ� WeaponInfo ���
    public WeaponInfo currentWeapon;      // ��ǰ����

    private int currentIndex = 0;

    private void Start()
    {
        // ���û���ֶ�ָ�������б������Զ������������ϵ����� WeaponInfo
        if (weapons == null || weapons.Length == 0)
            weapons = GetComponentsInChildren<WeaponInfo>();

        if (weapons.Length > 0)
        {
            currentWeapon = weapons[0];
            // ��ʼ����������Ҫ����������������Ƚ��ã���������
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

        //
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

    // 检查是否已有同类型武器
    for (int i = 0; i < weapons.Length; i++)
    {
        if (weapons[i] != null && weapons[i].weaponType == newWeaponInfo.weaponType)
        {
            // 替换：销毁当前手持的旧武器 GameObject
            if (weapons[i] != null)
                Destroy(weapons[i].gameObject);
            print($"替换武器：{weapons[i].name} -> {newWeaponInfo.name}");
            // 将新武器挂载到玩家武器容器下
            newWeaponObj.transform.SetParent(transform);
            newWeaponObj.transform.localPosition = Vector3.zero;
            newWeaponObj.transform.localRotation = Quaternion.identity;
            weapons[i] = newWeaponInfo;
            SwitchToWeapon(i);
            return true;
        }
    }

    print($"新增武器：{newWeaponInfo.name}");
    // 未拥有同类型，添加到数组
    newWeaponObj.transform.SetParent(transform);
    print(newWeaponObj.transform.parent.name +"是新武器的爸爸");
    newWeaponObj.transform.localPosition = Vector3.zero;
    newWeaponObj.transform.localRotation = Quaternion.identity;
    newWeaponObj.SetActive(false); // 先隐藏，切换时激活

    // 扩展数组（注意原数组可能有空位，先过滤掉 null）
    var list = new List<WeaponInfo>();
    foreach (var w in weapons)
        if (w != null) list.Add(w);
    list.Add(newWeaponInfo);
    weapons = list.ToArray();

    SwitchToWeapon(weapons.Length - 1);
    return true;
}

}


public struct WeaponChangedEvent
{
    public WeaponInfo newWeapon;
    public int weaponId;
}
