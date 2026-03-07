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
    public float damage = 10f;

    public GameObject DestoryEff;
    public GameObject damageTextPrefab;

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
        // 计算移动距离
        float moveDistance = Time.deltaTime * moveSpeed;
        // 射线检测，检测路径上的碰撞
        // 使用较小的距离增量，确保不会跳过碰撞
        float stepDistance = Mathf.Min(moveDistance, 0.1f);
        int steps = Mathf.CeilToInt(moveDistance / stepDistance);
        
        for (int i = 0; i < steps; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, stepDistance);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == "Enemy" || hit.collider.gameObject.tag == "Wall")
                {
                    DestroyMyself();
                    return;
                }
            }
            // 移动子弹一小步
            this.transform.Translate(transform.right * stepDistance, Space.World);
        }
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            // 显示伤害数字
            ShowDamageText(other.transform.position, damage);
            // 这里可以添加伤害处理逻辑
            DestroyMyself();
        }
        else if (other.gameObject.tag == "Wall")
        {
            //DestoryEff.gameObject.SetActive(true);
            //DestoryEff.transform.position = this.transform.position;
            DestroyMyself();
        }
    }
    
    // 显示伤害数字
    private void ShowDamageText(Vector3 position, float damageValue)
    {
        if (damageTextPrefab != null)
        {
            // 实例化伤害数字
            GameObject damageTextObj = Instantiate(damageTextPrefab, position, Quaternion.identity);
            // 设置伤害值文本
            UnityEngine.UI.Text textComponent = damageTextObj.GetComponent<UnityEngine.UI.Text>();
            if (textComponent != null)
            {
                textComponent.text = damageValue.ToString("0");
                // 根据伤害值设置颜色
                if (damageValue > 20)
                    textComponent.color = Color.red;
                else
                    textComponent.color = Color.white;
            }
            // 添加动画效果
            StartCoroutine(DamageTextAnimation(damageTextObj));
        }
    }
    
    // 伤害数字动画
    private IEnumerator DamageTextAnimation(GameObject damageTextObj)
    {
        Vector3 startPosition = damageTextObj.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, 1, 0); // 向上移动
        float duration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            // 平滑移动
            damageTextObj.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            // 逐渐透明
            UnityEngine.UI.Text textComponent = damageTextObj.GetComponent<UnityEngine.UI.Text>();
            if (textComponent != null)
            {
                Color color = textComponent.color;
                color.a = 1 - (elapsedTime / duration);
                textComponent.color = color;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 销毁伤害数字
        Destroy(damageTextObj);
    }
    private void PlayBulletEffect(GameObject effectPrefab)
    {
        print(effectPrefab.name);
        GameObject effect = Instantiate(effectPrefab, transform.position, transform.rotation);
        effect.AddComponent<AutoClear>();
        AudioSource audioSource = effect.GetComponent<AudioSource>();
        // ?0?0?1?7?1?7?1?7?0?1?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?7?0?6?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?7?1?6?1?7?1?7?1?7?1?7?1?7
        //SettingData settingData = PlayerPrefsDataManager.Instance.LoadData("Setting", typeof(SettingData)) as SettingData;
        //audioSource.volume = settingData.SoundToggle ? settingData.SoundVolume : 0;
        //audioSource.mute = !settingData.SoundToggle;
        audioSource.Play();
    }
}