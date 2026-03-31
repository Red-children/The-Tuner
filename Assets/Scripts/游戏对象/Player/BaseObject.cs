using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public abstract class BaseObject : MonoBehaviour
{
    // �������
    public int atk;
    
    public int maxHp;
    public int nowHp;

    // �ƶ����
    public float moveSpeed = 10;

    //������Ч
    public GameObject DiedEff;

    

    //������ ���������ʵ�����˺�����������
    public virtual void Wound(int damage )
    {
        //nowHp -= Mathf.Max(damage, 0);
        //if (nowHp <= 0)
        //{
        //    nowHp = 0;
        //    Died();
        //}
    }

    /// <summary>
    /// Ѫ��������������
    /// </summary>
    public virtual void Died()
    {
        //�ӳ������Ƴ�
        Destroy(this.gameObject);
        if (DiedEff != null)
        {
            GameObject gameObject = Instantiate(DiedEff, this.transform.position, this.transform.rotation);
            print(gameObject.name);
            //������Ч����������Ч
            //AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            //audioSource.volume = settingData.SoundToggle ? settingData.SoundVolume : 0;
            //audioSource.mute = !settingData.SoundToggle;
            //audioSource.Play();

            ////�������� 
            //SettingPlane[] settings = Object.FindSceneObjectsOfType(typeof(SettingPlane)) as SettingPlane[];
            //if (settings != null && settings.Length > 0)
            //{
            //    audioSource.volume = settings[0].MusicSlide.nowValue;
            //}
            ////�����Ƿ񲥷�
            //audioSource.mute = !settings[0].SettingData.SoundToggle;
            ////����û�й�ѡ Play On Awake
            //audioSource.Play();
        }
    }
}
