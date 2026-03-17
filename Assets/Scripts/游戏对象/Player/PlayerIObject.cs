using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Jobs;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public struct PlayerHitEvent
{
    public bool isCritical;   // �Ƿ�׼����
    public float damage;       // �˺�ֵ����ѡ��
}

public struct PlayerMeleeEvent
{
    public float damage;
    public Vector2 hitPoint;
}

public struct CameraShakeEvent
{
    public float intensity;   // ����ǿ�ȣ����Ը����˺�ֵ����
}

// public struct PlayerHealthChangedEventStruct
// {
//     public float currentHealth;
//     public float maxHealth;
//     public float healthPercent => currentHealth / maxHealth; //����UIֱ��ʹ��
// }

#region �������ݽ������ݵ��¼��ṹ�� ����Һ���������
public struct RhythmHitEvent
{
    public RhythmRank rank;      // �ж��ȼ�
    public float intensity;      // ���ݵȼ�������ǿ�ȣ���ѡ��


}
#endregion

public struct PlayerFireEvent
{
    public bool isPerfect;   // �Ƿ��������У���������ж��ȼ���
    public RhythmRank rank;   // ��ѡ�����ݾ���ȼ�
}



public struct PlayerDiedEvent
{
    public PlayerIObject player;
}


public class PlayerIObject : BaseObject
{
    [Header("��������")]
    public float dashDistance = 3f;          // ������ܾ���
    public float dashDuration = 0.3f;        // ���ܳ���ʱ��
    
    public bool isDashing = false;             // �Ƿ���������
    public AnimationCurve dashCurve;              // ����λ�����ߣ���ѡ�����ڿ������ܵļ���/����Ч����
    
    public float maxDashEnergy = 2;          // ����������
    public float currentDashEnergy = 2;   // ��ǰ������
    public float dashEnergyRegenRate = 1f;    // �������ָ����ʣ�ÿ��ָ����٣�
    public bool isDashOnWindow = false;             // �Ƿ��ڽ��ര���ڿ�������

    [Header("��ս��������")]
    public float meleeRange = 1.5f;          // ��ս��Χ
    public LayerMask enemyLayer;              // ���˲㼶
    public float meleeBaseDamage = 20f;       // �����˺�
    public float meleeCooldown = 0.5f;        // ��ս��ȴ
    private float lastMeleeTime = -999f;
    private float rhythmMultiplier = 1f; // Ĭ�ϱ���1
    


    [Header("Weapon")]
    public WeaponInfo currentWeapon;   // ��ǰʹ�õ�����
    
    public List<WeaponStats> weaponInfos;   // ���������б����� WeaponBase ��ȡ��

    public bool isInvincible { get; private set; }  // �Ƿ��޵�

    private float invincibleTimer;   //�޵м�ʱ��


    //������� ����׷�����λ��
    public Camera playerCamera;
    //ǽ�Ĳ㼶
    LayerMask wallLayer;

    //����ͷȡ���λ��
    public float offsetFactor = 0.3f;

    //����ƶ���ƽ����
    public float cameraSmoothness = 5f;
    //�����z��λ�ã�ȷ����������ǰ��
    public float cameraZ = -10f;

    //��ҵ�SpriteRenderer��� ���ڷ�ת��ɫ
    public SpriteRenderer spriteRenderer;

    public void Start()
    {
        //��ʼ����һ���¼� ��UI����ȷ��ʾ��ʼѪ��
        EventBus.Instance.Trigger<PlayerHealthChangedEventStruct>(new PlayerHealthChangedEventStruct
        {
            currentHealth = nowHp,
            maxHealth = maxHp

        }); 

        #region ��ʼ��
        //�õ�ǽ�Ĳ㼶 �����Ż������ײ
        wallLayer = LayerMask.GetMask("Wall");
        //������ǰ����ľ���
        cameraZ = playerCamera.transform.position.z;
        //������ҹ���������ǰ����
        passPlayerAtk();
        // �ӵ�ǰ�����󶨵� WeaponBase �л�ȡ�����б�
        weaponInfos = currentWeapon.weaponBase.weaponList;
        // ��ʼ����������
        currentWeapon.InitializeWeapon(currentWeapon.weaponType);
        #endregion

    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<RhythmData>(OnRhythmData);
    }
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<RhythmData>(OnRhythmData);
    }
    private void OnRhythmData(RhythmData data)
    {
        rhythmMultiplier = (float)data.multiplier;
        isDashOnWindow = data.isInWindow; // ���ݽ��ര��״̬�������ܿ�����
    }


    #region ��д���˷���


    // ��д Wound ����  �����˺���ֵ
    public override void Wound(int damage)
    {

        if (isInvincible || nowHp <= 0) return;  // �޵л��������򲻴���

        // ��Ѫ
        nowHp -= Mathf.Max(damage, 0);
        Debug.Log($"������ˣ���ǰѪ��: {nowHp}");

        EventBus.Instance.Trigger<PlayerHealthChangedEventStruct>(new PlayerHealthChangedEventStruct
        {
            currentHealth = nowHp,
            maxHealth = maxHp

        });

        // ���������¼���ǿ�ȿ��Լ���Ϊ damage / 10f�����������ֵ������
        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = damage / 100f });

        // �����޵�֡
        StartCoroutine(InvincibilityCoroutine(1f)); // �޵�1��

        if (nowHp <= 0)
        {
            nowHp = 0;
            Died();  // �����Լ���������������������д�� Died��
        }
    }
    #endregion  

    #region �޵�֡��Э�̺���


    // �޵�Э�� �������ʱ��
    private IEnumerator InvincibilityCoroutine(float duration)
    {
        // ��ʼ�޵�
        isInvincible = true;

        //ʱ�������
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // ÿ0.1����˸һ�Σ�ʾ����
            // ����������� SpriteRenderer ��͸���Ȼ���ɫ
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        isInvincible = false;
    }
    #endregion

    #region ��д�������� ������������¼�


    // ��д������������ѡ��
    public override void Died()
    {
       
        Debug.Log("�������");
        // ������������¼�����UI����Ч�ȼ�����
        EventBus.Instance.Trigger(new PlayerDiedEvent { player = this });
        // �����������������ٵȣ����ݲ����٣�������ʾ GameOver ���棩
        base.Died();  // �����ٶ�����������������٣�����ע�� base.Died()
    }
    #endregion

    #region ���Ѫ���仯�¼�
    public void PlayerHpChange(PlayerHealthChangedEventStruct playerHealthChangedEventStruct)
    {
       playerHealthChangedEventStruct.currentHealth = nowHp;
       playerHealthChangedEventStruct.maxHealth = maxHp;
    }
    #endregion

    #region ��Ҵ��ݹ�����������
    // ���ݹ���������ǰ����
    public void passPlayerAtk()
    {
        currentWeapon.ownerDamage = this.atk;
    }
    #endregion

    #region ��ս�������� �����˺���������
    private void MeleeAttack()
    {
        lastMeleeTime = Time.time;

        // ���������˺�
        float finalDamage = (atk + meleeBaseDamage) * rhythmMultiplier;

        // ������
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, meleeRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            FSM enemyFSM = enemy.GetComponent<FSM>();
            if (enemyFSM != null)
            {
                enemyFSM.Wound(finalDamage);
            }
        }

        EventBus.Instance.Trigger(new CameraShakeEvent { intensity = finalDamage * 0.1f }); // ʾ��ǿ��

        // ������ս�¼�������Ч��ʹ�ã�
        EventBus.Instance.Trigger(new PlayerMeleeEvent { damage = finalDamage, hitPoint = transform.position });

    }
    #endregion

    public void Update()
    {
        #region �ƶ��߼�
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        float rayLengthX = 0.9f; // �Դ�����Ұ뾶
        float rayLengthY = 0.9f;
        wallLayer = LayerMask.GetMask("Wall");

        // �ֱ���X��Y���򣬱���Խ���ͬʱ����
        if (moveX != 0)
        {
            RaycastHit2D hitX = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(moveX), rayLengthX, wallLayer);
            if (hitX.collider != null) moveX = 0;
        }
        if (moveY != 0)
        {
            RaycastHit2D hitY = Physics2D.Raycast(transform.position, Vector2.up * Mathf.Sign(moveY), rayLengthY, wallLayer);
            if (hitY.collider != null) moveY = 0;
        }

        // Ӧ���ƶ�
        transform.Translate(new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime, Space.World);


        #endregion

        #region ������

        //�������ü��


        if (Input.GetMouseButton(0))
            currentWeapon.Shoot();
        // �л����������ּ���
        if (Input.GetKeyDown(KeyCode.Alpha1))
            currentWeapon.SwitchWeapon(WeaponType.Pistol);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            currentWeapon.SwitchWeapon(WeaponType.Shotgun);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            currentWeapon.SwitchWeapon(WeaponType.Rifle);
        #endregion

        #region ���׷���߼�
        // ��ȡ���������ռ��е�λ�ã�ע�⣺ScreenToWorldPoint ��Ҫ��ȷ��Zֵ��
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z; // ʹ�ý�ɫ����Ļ���
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // �����������ӽ�ɫָ����꣩
        Vector2 directionMouse = mouseWorldPos - transform.position;

        //// ������ͽ�ɫ�غϣ�����ת
        //if (directionMouse.magnitude > 0.01f)
        //{
        //    // ���㷽����X��ļнǣ����ȣ���תΪ�Ƕ�
        //    float angle = Mathf.Atan2(directionMouse.y, directionMouse.x) * Mathf.Rad2Deg;
        //    //ת��
        //    transform.rotation = Quaternion.Euler(0, 0, angle);
        //}

        // �����׷���߼������ӣ�
        if (directionMouse.x > 0)
            spriteRenderer.flipX = false; // ����
        else if (directionMouse.x < 0)
            spriteRenderer.flipX = true;  // ����       
                            // ע�⣺���������X�᷽�������ת�������������Ϸ����ᱣ���ϴγ��򣬵�ͨ�����á�
                                          // ����ϸ�Ŀ��Խ�Ϸ���Ƕȣ�����������
        #endregion

        #region ��ս�������
        if (Input.GetKeyDown(KeyCode.V) && Time.time > lastMeleeTime + meleeCooldown)
        {
            MeleeAttack();
        }
        #endregion


        // ����ָ�����
        if (currentWeapon != null)
        {
            Vector2 weaponDir = directionMouse; // ��������ҵ����һ��
            float weaponAngle = Mathf.Atan2(weaponDir.y, weaponDir.x) * Mathf.Rad2Deg;
            currentWeapon.transform.rotation = Quaternion.Euler(0, 0, weaponAngle);
        }


        #region �����л�
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeapon.SwitchWeapon(WeaponType.Pistol);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon.SwitchWeapon(WeaponType.Shotgun);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentWeapon.SwitchWeapon(WeaponType.Rifle);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            var weaponList = currentWeapon.weaponBase.weaponList;
            int count = weaponList.Count;
            if (count == 0) return; // ��ȫ����

            // ͨ����ǰ���������Ͳ������б��е�����
            int currentIndex = weaponList.FindIndex(w => w.weaponType == currentWeapon.weaponType);
            if (currentIndex == -1)
            {
                Debug.LogWarning("��ǰ�������Ͳ��������б��У�Ĭ���л�����һ��");
                currentIndex = 0;
            }

            int delta = scroll > 0 ? 1 : -1;
            // (currentIndex + delta + count) % count ��֤����� [0, count-1] ֮��
            int newIndex = (currentIndex + delta + count) % count;

            WeaponType newType = weaponList[newIndex].weaponType;
            currentWeapon.SwitchWeapon(newType);
        }
        #endregion

        #region  ��������
        if (currentDashEnergy < maxDashEnergy)
        {
            currentDashEnergy += dashEnergyRegenRate * Time.deltaTime;
            if (currentDashEnergy > maxDashEnergy)
                currentDashEnergy = maxDashEnergy;
        }

        // 2. �������ܷ���
        Vector2 dashDir;
        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f)
        {
            // ���ƶ����룺ʹ��WASD���򣨹�һ����
            dashDir = new Vector2(moveX, moveY).normalized;
        }
        else
        {
            // ���ƶ����룺ʹ����귽��
            dashDir = directionMouse.normalized; // directionMouse ����ǰ�����
        }
        // 3. ��������Ŀ��㣨����ǽ�ڣ�
        Vector3 targetPos = transform.position + (Vector3)dashDir * dashDistance;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDir, dashDistance, wallLayer);
        if (hit.collider != null)
        {
            Vector2 adjustedPoint = hit.point - dashDir * 0.2f;
            targetPos = new Vector3(adjustedPoint.x, adjustedPoint.y, transform.position.z);
        }

        // 4. ��������
        if (Input.GetMouseButtonDown(1) && (currentDashEnergy > 1 || isDashOnWindow))
        {
            if (!isDashOnWindow)
            {
                currentDashEnergy -= 1;
            }
            StartCoroutine(DashCoroutine(transform.position, targetPos, dashDuration));
        }
        #endregion


    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }

    private IEnumerator DashCoroutine(Vector3 start, Vector3 target, float duration)
    {
        isDashing = true;
        isInvincible = true;

         
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveT = dashCurve.Evaluate(t);
            transform.position = Vector3.Lerp(start, target, curveT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target; // ȷ������λ��׼ȷ

        isDashing = false;
        isInvincible = false;
    }

}



    

