using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum E_BulletType 
{
    Red,
    Green,
};

public struct BulletData
{
    public int id;
    public string resPath;
    public E_BulletType bulletType;
}

public class Bullet : MonoBehaviour
{

    public int moveSpeed=10;

    public GameObject DestoryEff;

    public void DestroyMyself()   
    {
        PlayBulletEffect(DestoryEff.gameObject);
        Destroy(this.gameObject); 
    }
    private void Start()
    {
         DestoryEff = Resources.Load<GameObject>("Eff") as GameObject;
    }
    void Update()
    {
        this.transform.Translate(this.transform.right * Time.deltaTime * moveSpeed, Space.World);
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            DestroyMyself();
        }
        else if (other.gameObject.tag == "Wall")
        {
            //DestoryEff.gameObject.SetActive(true);
            //DestoryEff.transform.position = this.transform.position;
            DestroyMyself();
        }
        // 处理可破坏物体
        DstrObjInfo destructible = other.GetComponent<DstrObjInfo>();
        if (destructible != null)
        {
            destructible.TakeDamage(damage); // 扣血
            DestroyMyself();                  // 子弹消失
        }
    }
    private void PlayBulletEffect(GameObject effectPrefab)
    {
        print(effectPrefab.name);
        GameObject effect = Instantiate(effectPrefab, transform.position, transform.rotation);
        effect.AddComponent<AutoClear>();
        AudioSource audioSource = effect.GetComponent<AudioSource>();
        // 使用已缓存的数据管理器实例，避免重复加载
        //SettingData settingData = PlayerPrefsDataManager.Instance.LoadData("Setting", typeof(SettingData)) as SettingData;
        //audioSource.volume = settingData.SoundToggle ? settingData.SoundVolume : 0;
        //audioSource.mute = !settingData.SoundToggle;
        audioSource.Play();
    }
}
