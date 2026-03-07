using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public abstract class BaseObject : MonoBehaviour
{
    // 属性相关
    public int atk;
    
    public int maxHp;
    public int nowHp;

    // 移动相关
    public float moveSpeed = 10;

    //死亡特效
    public GameObject DiedEff;

    

    /// <summary>
    ///  受伤判断方法
    /// </summary>
    /// <param name="other"></param>
    public virtual void Wound(BaseObject other)
    {

        nowHp -= Mathf.Max(other.atk, 0);
        if (nowHp <= 0)
        {
            nowHp = 0;
            Died();
        }
    }

    /// <summary>
    /// 血量归零死亡方法
    /// </summary>
    public virtual void Died()
    {
        //从场景中移除
        Destroy(this.gameObject);
        if (DiedEff != null)
        {
            GameObject gameObject = Instantiate(DiedEff, this.transform.position, this.transform.rotation);
            print(gameObject.name);
            //由于特效本身带有音效
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            //audioSource.volume = settingData.SoundToggle ? settingData.SoundVolume : 0;
            //audioSource.mute = !settingData.SoundToggle;
            audioSource.Play();

            ////设置音量 
            //SettingPlane[] settings = Object.FindSceneObjectsOfType(typeof(SettingPlane)) as SettingPlane[];
            //if (settings != null && settings.Length > 0)
            //{
            //    audioSource.volume = settings[0].MusicSlide.nowValue;
            //}
            ////设置是否播放
            //audioSource.mute = !settings[0].SettingData.SoundToggle;
            ////避免没有勾选 Play On Awake
            //audioSource.Play();
        }
    }
}
