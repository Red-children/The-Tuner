using UnityEngine;

public class WeaponSoundManager : MonoBehaviour
{
    [Header("Snapfingers音效")]
    public AudioClip snapfingers1;
    public AudioClip snapfingers2;
    public AudioClip snapfingers3;
    
    [Header("音频设置")]
    [Range(0f, 1f)] public float volume = 1f;
    
    private AudioSource audioSource;
    private int currentSnapIndex = 0; // 当前播放的音效索引
    private AudioClip[] snapClips;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // 初始化音效数组
        snapClips = new AudioClip[3];
    }
    
    private void Start()
    {
        // 如果没有在Inspector中赋值，尝试从Resources加载
        if (snapfingers1 == null)
            snapfingers1 = Resources.Load<AudioClip>("sounds/snapfingers1");
        if (snapfingers2 == null)
            snapfingers2 = Resources.Load<AudioClip>("sounds/snapfingers2");
        if (snapfingers3 == null)
            snapfingers3 = Resources.Load<AudioClip>("sounds/snapfingers3");
        
        // 填充数组
        snapClips[0] = snapfingers1;
        snapClips[1] = snapfingers2;
        snapClips[2] = snapfingers3;
    }
    
    /// <summary>
    /// 轮流播放snapfingers音效
    /// </summary>
    public void PlaySnapSound()
    {
        if (snapClips == null || snapClips.Length == 0)
        {
            Debug.LogWarning("[WeaponSoundManager] 没有设置snapfingers音效");
            return;
        }
        
        // 获取当前要播放的音效
        AudioClip clipToPlay = snapClips[currentSnapIndex];
        
        if (clipToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay, volume);
            Debug.Log($"[WeaponSoundManager] 播放音效: snapfingers{currentSnapIndex + 1}");
        }
        
        // 切换到下一个音效索引 (0 -> 1 -> 2 -> 0)
        currentSnapIndex = (currentSnapIndex + 1) % snapClips.Length;
    }
    
    /// <summary>
    /// 重置音效索引
    /// </summary>
    public void ResetSnapIndex()
    {
        currentSnapIndex = 0;
    }
    
    /// <summary>
    /// 设置音量
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
    }
}
