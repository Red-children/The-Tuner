using UnityEngine;

/// <summary>
/// 歌曲数据配置类，包含BPM、音频资源和采样设置等信息，同样挂载在PreciseBGMManager上，供主控和进程管理器使用
/// </summary>
[System.Serializable]

public class BgmSongData : MonoBehaviour
{
    [Tooltip("歌曲BPM(每分钟节拍数)")]
    public double BPM = 120;

    [Header("【音频资源】")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioClip bgmClip;

    [Header("【采样配置】")]
    [Tooltip("采样间隔(毫秒),建议10ms(100Hz)兼顾精度和性能")]
    [Range(1, 20)] public int sampleIntervalMs = 10;

    [Tooltip("第一拍位置（秒）")]
    public double firstOffset = 0;
    

    // 对外提供只读访问（保护数据不被外部修改）
    public AudioSource BgmAudioSource => bgmAudioSource;
    public AudioClip BgmClip => bgmClip;

    private void Awake()
    {
        // 初始化音频源基础配置
        if (bgmAudioSource != null)
        {
            bgmAudioSource.loop = false;        // 循环由主控逻辑控制
            bgmAudioSource.spatialBlend = 0;    // 2D音频
            bgmAudioSource.playOnAwake = false; // 不自动播放
            bgmAudioSource.clip = bgmClip;      // 预设音频剪辑
            bgmAudioSource.volume = 1f;         // 默认全音量，主控可调整
        }
    }
}