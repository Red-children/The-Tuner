using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct PlayBGMEvent
{
    //  TODO:
    public float time; // 事件发生时间
}
struct BGMProgressUpdateEvent
{
    //  TODO:
}

public class PreciseBGMManager : MonoBehaviour
{
    [SerializeField] private AudioSource _bgmAudioSource;
    [SerializeField] private AudioClip _bgmClip;
    [Tooltip("采样间隔(毫秒),音游建议10ms(100Hz)兼顾精度和性能")]
    [Range(1, 20)] public int sampleIntervalMs = 10;

/************/
    // 音频时钟（DSP Time）：和音频硬件同步，无帧延迟
    private double _dspStartTime; // BGM开始播放时的DSP时间
    private bool _isPlaying = false;

    private void OnPlayBGM(PlayBGMEvent evt)
    {
        Debug.Log("BGMTest:Received");
        if (_bgmAudioSource.isPlaying)
        {
            Debug.Log("BGMTest isPlaying");
            return;
        }
        // 记录DSP开始时间（音频硬件的时间，比Time.time精准）
        _dspStartTime = AudioSettings.dspTime;
        _bgmAudioSource.PlayScheduled(_dspStartTime); // 精准调度播放
        _isPlaying = true;
        // 启动高频采样协程（替代Update，减少空判断）
        StartCoroutine(PreciseProgressSampler());

    }
    private IEnumerator PreciseProgressSampler()
    {
                // 转换采样间隔为秒
        float interval = sampleIntervalMs / 1000f;
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(interval); // 不受Time.timeScale影响

        while (_isPlaying && _bgmAudioSource.isPlaying)
        {
            // 核心：用DSP时间计算精准进度（无帧延迟）
            double currentDspTime = AudioSettings.dspTime;
            float preciseTime = (float)(currentDspTime - _dspStartTime);
            // 防止进度超过总时长（音频结束前的最后一帧）
            preciseTime = Mathf.Clamp(preciseTime, 0, _bgmClip.length);
            
            // 计算进度数据
            float progress = preciseTime / _bgmClip.length;

            // 推送精准进度事件（复用之前的事件类）
            BGMProgressUpdateEvent progressEvent = new BGMProgressUpdateEvent
            {
                //  TODO:
                // Progress = progress,
                // CurrentTime = preciseTime,
                // TotalDuration = _bgmClip.length
            };
            PreciseEventBus.Instance.Trigger(progressEvent);

            yield return wait;
        }

        _isPlaying = false;
    }

    private void InitializeManager()
    {
        // 关键配置：音游BGM必须用Streaming加载，且禁用空间音效
        _bgmAudioSource.loop = false;
        _bgmAudioSource.spatialBlend = 0;
        _bgmAudioSource.playOnAwake = false;
        _bgmAudioSource.clip = _bgmClip;
        _bgmAudioSource.volume = 1f;

        // 订阅播放事件
        PreciseEventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);
    }

    public void StopBGM()
    {
        _isPlaying = false;
        _bgmAudioSource.Stop();
    }

    private void OnDestroy()
    {
        // 取消订阅，清理资源
        PreciseEventBus.Instance.Unsubscribe<PlayBGMEvent>(OnPlayBGM);
        _isPlaying = false;
        StopCoroutine(PreciseProgressSampler());
    }

    void Awake()
    {
        InitializeManager();
    }

}
