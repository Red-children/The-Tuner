using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum BulletOwner
{
    Player,
    Enemy
}



public class Bullet : MonoBehaviour
{

    public int moveSpeed = 10;
    public float damage = 10f;

    public BulletOwner owner;          // 由生成时设置
    public GameObject DestoryEff;
   
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
        float moveDistance = Time.deltaTime * moveSpeed;
        float stepDistance = Mathf.Min(moveDistance, 0.1f);
        int steps = Mathf.CeilToInt(moveDistance / stepDistance);

        // 根据所有者决定检测层
        int layerMask;
        if (owner == BulletOwner.Player)
            layerMask = LayerMask.GetMask("Enemy", "Wall");
        else
            layerMask = LayerMask.GetMask("Player", "Wall");

        for (int i = 0; i < steps; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, stepDistance, layerMask);
            if (hit.collider != null)
            {
                // 玩家子弹击中敌人
                if (owner == BulletOwner.Player && hit.collider.CompareTag("Enemy"))
                {
                    hit.collider.GetComponent<FSM>()?.Wound(damage);
                    DestroyMyself();
                    return;
                }
                // 敌人子弹击中玩家
                else if (owner == BulletOwner.Enemy && hit.collider.CompareTag("Player"))
                {
                    hit.collider.GetComponent<PlayerIObject>()?.Wound((int)damage);
                    DestroyMyself();
                    return;
                }
                // 任何子弹击中墙壁
                else if (hit.collider.CompareTag("Wall"))
                {
                    DestroyMyself();
                    return;
                }
            }
            transform.Translate(transform.right * stepDistance, Space.World);
        }
    }



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

        #endregion

    }
}