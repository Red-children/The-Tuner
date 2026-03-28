using UnityEngine;


public class PlayerWeapon : MonoBehaviour
{
    [Header("武器列表（可选，从子物体自动获取）")]
    public WeaponInfo[] weapons;          // 可手动拖拽所有武器
    public WeaponInfo currentWeapon;      // 当前武器

    private int currentIndex = 0;

    private void Start()
    {
        // 如果没有手动指定武器列表，则自动查找子物体上的所有 WeaponInfo
        if (weapons == null || weapons.Length == 0)
            weapons = GetComponentsInChildren<WeaponInfo>();

        if (weapons.Length > 0)
        {
            currentWeapon = weapons[0];
            // 初始武器可能需要激活，其他武器可以先禁用（根据需求）
            foreach (var w in weapons) w.gameObject.SetActive(false);
            currentWeapon.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // 数字键切换
        for (int i = 0; i < weapons.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchToWeapon(i);
                break;
            }
        }


        Vector2 directionMouse =  Camera.main.ScreenToWorldPoint(Input.mousePosition) - currentWeapon.transform.position;
        // 滚轮切换
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && weapons.Length > 0)
        {
            int delta = scroll > 0 ? 1 : -1;
            int newIndex = (currentIndex + delta + weapons.Length) % weapons.Length;
            SwitchToWeapon(newIndex);
        }

        // 武器指向鼠标
        if (currentWeapon != null)
        {
            Vector2 weaponDir = directionMouse; // 方向与玩家到鼠标一致
            float weaponAngle = Mathf.Atan2(weaponDir.y, weaponDir.x) * Mathf.Rad2Deg;
            currentWeapon.transform.rotation = Quaternion.Euler(0, 0, weaponAngle);
        }

    }

    private void SwitchToWeapon(int index)
    {
        if (index == currentIndex) return;
        if (index < 0 || index >= weapons.Length) return;

        // 禁用当前武器，启用新武器
        currentWeapon.gameObject.SetActive(false);
        currentWeapon = weapons[index];
        currentWeapon.gameObject.SetActive(true);
        currentIndex = index;

        // 发布武器切换事件，供 UI 等模块更新
        EventBus.Instance.Trigger(new WeaponChangedEvent
        {
            newWeapon = currentWeapon,
        });
    }

    //对外接口
    public WeaponInfo GetCurrentWeapon()
    {
        return currentWeapon;
    }
}

// 武器切换事件结构体（需放在合适的位置，比如 UIEvents.cs）
public struct WeaponChangedEvent
{
    public WeaponInfo newWeapon;
    public int weaponId;
}