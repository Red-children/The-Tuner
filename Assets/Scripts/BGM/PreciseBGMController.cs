using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PreciseBGMController : MonoBehaviour
{
    [SerializeField] private BgmSongData songData;
    [SerializeField] private BgmProgressManager _progressManager;

    [Header("音量控制")]
    [SerializeField] private float bgmVolume = 0.1f;

    [Header("淡入淡出时间")]
    public float defaultFadeDuration = 0.5f;

    private Coroutine _progressSamplerCoroutine;
    private Tweener _fadeTween;

    #region 生命周期
    private void Awake()
    {
        var existing = FindObjectOfType<PreciseBGMController>();
        if (existing != null && existing != this)
        {
            Destroy(gameObject);
            return;
        }

        AutoGetSubModules();
        _progressManager?.Init(songData);

        EventBus.Instance.Subscribe<PlayBGMEvent>(OnPlayBGM);
        EventBus.Instance.Subscribe<BGMChangeEvent>(OnBGMChange); // ✅ 订阅

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        EventBus.Instance.Unsubscribe<PlayBGMEvent>(OnPlayBGM);
        EventBus.Instance.Unsubscribe<BGMChangeEvent>(OnBGMChange); // ✅ 取消
        KillFadeTween();
        _progressManager?.StopBgmPlay();
    }
    #endregion

    #region 事件处理
    private void OnPlayBGM(PlayBGMEvent evt)
    {
        if (_progressManager != null && _progressManager.IsPlaying)
            return;

        _progressManager?.StartBgmPlay();

        if (RhythmManager.Instance != null && songData != null)
        {
            RhythmManager.Instance.bpm = songData.GetBPM();
            RhythmManager.Instance.StartRhythm(_progressManager.DspStartTime, songData.GetFirstOffset());
        }

        if (_progressSamplerCoroutine != null)
            StopCoroutine(_progressSamplerCoroutine);

        _progressSamplerCoroutine = StartCoroutine(PreciseProgressSampler());
    }

    // ✅ 只接收 BGMData，完美！
    private void OnBGMChange(BGMChangeEvent evt)
    {
        if (evt.songData == null) return;

        FadeOutBGM(defaultFadeDuration, () =>
        {
            SwitchBGM(evt.songData, evt.startRhythm);
        });
    }
    #endregion

    #region 核心切换逻辑（只使用 BGMData）
    public void SwitchBGM(BGMData data, bool startRhythm = false)
    {
        songData.SwitchBGM(data);
        _progressManager.Init(songData);

        FadeInBGM(defaultFadeDuration, () =>
        {
            if (startRhythm)
            {
                RhythmManager.Instance.bpm = songData.GetBPM();
                RhythmManager.Instance.StartRhythm(_progressManager.DspStartTime, songData.GetFirstOffset());
            }
            else
            {
                RhythmManager.Instance?.StopRhythm();
            }
        });
    }
    #endregion

    #region 淡入淡出
    public void FadeOutBGM(float duration, Action onComplete = null)
    {
        if (songData == null || songData.BgmAudioSource == null) return;

        KillFadeTween();
        _fadeTween = songData.BgmAudioSource.DOFade(0, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                StopBGM();
                songData.BgmAudioSource.volume = _progressManager != null ? _progressManager.EffectiveVolume : bgmVolume;
                onComplete?.Invoke();
            });
    }

    public void FadeInBGM(float duration, Action onComplete = null)
    {
        if (songData == null || songData.BgmAudioSource == null) return;

        KillFadeTween();
        StopBGM();

        float targetVol = _progressManager != null ? _progressManager.EffectiveVolume : bgmVolume;
        songData.BgmAudioSource.volume = 0;
        songData.BgmAudioSource.Play();
        _progressManager?.MarkAsPlaying();

        if (_progressSamplerCoroutine != null)
            StopCoroutine(_progressSamplerCoroutine);

        _progressSamplerCoroutine = StartCoroutine(PreciseProgressSampler());

        _fadeTween = songData.BgmAudioSource.DOFade(targetVol, duration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }
    public void FadeInBGM(BGMData data, float duration, Action onComplete = null)
    {
        if (songData == null || songData.BgmAudioSource == null) return;

        KillFadeTween();
        StopBGM();

        // 切换BGM数据
        songData.SwitchBGM(data);
        _progressManager.Init(songData);

        float targetVolume = _progressManager != null ? _progressManager.EffectiveVolume : bgmVolume;
        songData.BgmAudioSource.volume = 0f;
        songData.BgmAudioSource.Play();
        _progressManager?.MarkAsPlaying();

        // 重启节奏
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.bpm = songData.GetBPM();
            RhythmManager.Instance.StartRhythm(_progressManager.DspStartTime, songData.GetFirstOffset());
        }

        if (_progressSamplerCoroutine != null)
            StopCoroutine(_progressSamplerCoroutine);
        _progressSamplerCoroutine = StartCoroutine(PreciseProgressSampler());

        _fadeTween = songData.BgmAudioSource.DOFade(targetVolume, duration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }
    #endregion

    #region 工具方法
    private void AutoGetSubModules()
    {
        if (songData == null) songData = GetComponent<BgmSongData>();
        if (_progressManager == null) _progressManager = GetComponent<BgmProgressManager>();
    }

    public void StopBGM()
    {
        _progressManager?.StopBgmPlay();
        if (_progressSamplerCoroutine != null)
        {
            StopCoroutine(_progressSamplerCoroutine);
            _progressSamplerCoroutine = null;
        }
    }

    private IEnumerator PreciseProgressSampler()
    {
        if (songData == null) yield break;
        var wait = new WaitForSecondsRealtime(songData.sampleIntervalMs / 1000f);

        while (_progressManager != null && _progressManager.IsPlaying)
        {
            _progressManager.UpdatePreciseProgress();

            if (_progressManager.IsBgmFinished())
            {
                _progressManager.StopBgmPlay();
                RhythmManager.Instance?.StopRhythm();
                break;
            }

            yield return wait;
        }

        _progressSamplerCoroutine = null;
    }

    private void KillFadeTween()
    {
        if (_fadeTween != null && _fadeTween.IsActive()) _fadeTween.Kill();
        _fadeTween = null;
    }
    #endregion
}