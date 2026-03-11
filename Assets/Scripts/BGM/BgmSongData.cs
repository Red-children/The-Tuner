using UnityEngine;

// 歌曲信息序列化模块（仅存储配置，无逻辑）
[System.Serializable]
public class BgmSongData : MonoBehaviour
{
    [Header("【歌曲核心配置】")]
    [Tooltip("伤害倍率（精准/普通）")]
    public float[] multiplierArray = { 0.99f, 0.01f };
    [Tooltip("精准判定区间（秒）")]
    public double windowPeriod = 0.2f;
    [Tooltip("歌曲BPM(每分钟节拍数)")]
    public double BPM = 120;
    [Tooltip("前N秒不触发指示器(秒)")]
    public double firstOffset = 1.1f;

    [Header("【音频资源】")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioClip bgmClip;

    [Header("【采样配置】")]
    [Tooltip("采样间隔(毫秒),建议10ms(100Hz)兼顾精度和性能")]
    [Range(1, 20)] public int sampleIntervalMs = 10;

    // 对外提供只读访问（保护数据不被外部修改）
    public AudioSource BgmAudioSource => bgmAudioSource;
    public AudioClip BgmClip => bgmClip;

    private void Awake()
    {
        // 初始化音频源基础配置
        if (bgmAudioSource != null)
        {
            bgmAudioSource.loop = false;
            bgmAudioSource.spatialBlend = 0;
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.clip = bgmClip;
            bgmAudioSource.volume = 1f;
        }
    }
}