using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;





public class Bullet : MonoBehaviour
{

    public int moveSpeed = 10;
    public float damage = 10f;

    public GameObject DestoryEff;
    public GameObject damageTextPrefab; // 死亡飘字预制体

    private bool hasDealtDamage = false; // 是否已处理伤害


    #region 子弹销毁方法


    public void DestroyMyself()
    {

        PlayBulletEffect(DestoryEff.gameObject);//播放子弹特效
        Destroy(this.gameObject);
    }
    #endregion

    private void Start()
    {
        #region 初始化

        DestoryEff = Resources.Load<GameObject>("Eff") as GameObject; //子弹摧毁特效初始化

        #endregion

    }
    void Update()
    {
        #region MyRegion


        // 计算移动距离
        float moveDistance = Time.deltaTime * moveSpeed;
        // 射线检测，检测路径上的碰撞
        // 使用较小的距离增量，确保不会跳过碰撞
        float stepDistance = Mathf.Min(moveDistance, 0.1f);
        int steps = Mathf.CeilToInt(moveDistance / stepDistance);

        for (int i = 0; i < steps; i++)
        {
            int layerMask = LayerMask.GetMask("Enemy", "Wall");
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, stepDistance, layerMask);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == "Enemy" && !hasDealtDamage)
                {
                    hasDealtDamage = true;
                    hit.collider.gameObject.GetComponent<FSM>()?.Wound(damage);
                    ShowDamageText(hit.collider.transform.position, damage);
                    DestroyMyself();
                    return;
                }
                else if (hit.collider.gameObject.tag == "Wall")
                {
                    DestroyMyself();
                    return;
                }
            }
            transform.Translate(transform.right * stepDistance, Space.World);
        }
    }


    #endregion

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !hasDealtDamage)
        {
            hasDealtDamage = true;
            other.GetComponent<FSM>()?.Wound(damage);  // 添加受伤调用
            ShowDamageText(other.transform.position, damage);
            DestroyMyself();
        }
        else if (other.gameObject.tag == "Wall")
        {
            DestroyMyself();
        }

        DstrObjInfo destructible = other.GetComponent<DstrObjInfo>();
        if (destructible != null)
        {
            destructible.TakeDamage(damage); // 扣血
            DestroyMyself();                  // 子弹消失
        }
    }

    #region 显示伤害飘字
    // 显示伤害数字
    private void ShowDamageText(Vector3 position, float damageValue)
    {
        if (damageTextPrefab == null) return;
        // 实例化预制体，位置直接使用敌人位置
        GameObject dmgObj = Instantiate(damageTextPrefab, position, Quaternion.identity);
        // 获取 DamageNumber 组件（挂在 Canvas 上）
        DamageNumber dmgNumber = dmgObj.GetComponent<DamageNumber>();
        if (dmgNumber != null)
        {
            // 将伤害值传递给 DamageNumber
            dmgNumber.SetDamage(damageValue);
        }
    }
    #endregion

    #region 播放死亡特效方法
    private void PlayBulletEffect(GameObject effectPrefab)
    {
        print(effectPrefab.name);
        GameObject effect = Instantiate(effectPrefab, transform.position, transform.rotation);
        effect.AddComponent<AutoClear>();
        AudioSource audioSource = effect.GetComponent<AudioSource>();
        //SettingData settingData = PlayerPrefsDataManager.Instance.LoadData("Setting", typeof(SettingData)) as SettingData;
        //audioSource.volume = settingData.SoundToggle ? settingData.SoundVolume : 0;
        //audioSource.mute = !settingData.SoundToggle;
        audioSource.Play();
    }
    #endregion
}