using UnityEngine;
using DG.Tweening;

/// <summary>
/// MainMenu 场景背景语音/音乐淡出控制器。
/// 挂载到场景中播放背景语音的 AudioSource 同一对象上，或挂载到任意对象并手动绑定 AudioSource。
/// </summary>
public class MainMenuVoiceFader : MonoBehaviour
{
    [Header("音频源")]
    [SerializeField] private AudioSource voiceSource;

    [Header("淡出参数")]
    [Tooltip("淡出持续时间（秒）")]
    [SerializeField] private float fadeDuration = 1.5f;
    [Tooltip("淡出后是否自动停止 AudioSource")]
    [SerializeField] private bool stopAfterFade = true;
    [Tooltip("淡出曲线")]
    [SerializeField] private Ease fadeEase = Ease.OutQuad;

    private Tweener _fadeTween;

    #region 生命周期

    private void Awake()
    {
        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();

        if (voiceSource == null)
            Debug.LogError($"MainMenuVoiceFader: {gameObject.name} 上未找到 AudioSource 组件！", gameObject);
    }

    private void OnDestroy()
    {
        KillFadeTween();
    }

    #endregion

    #region 对外接口

    /// <summary> 播放背景语音（从头开始） </summary>
    public void PlayVoice()
    {
        if (voiceSource == null) return;
        KillFadeTween();
        voiceSource.volume = 1f;
        voiceSource.Play();
    }

    /// <summary> 淡出背景语音 </summary>
    /// <param name="onComplete">淡出完成后的回调</param>
    public void FadeOutVoice(System.Action onComplete = null)
    {
        FadeOutVoice(fadeDuration, onComplete);
    }

    /// <summary> 淡出背景语音（自定义时长） </summary>
    /// <param name="duration">淡出时长</param>
    /// <param name="onComplete">淡出完成后的回调</param>
    public void FadeOutVoice(float duration, System.Action onComplete = null)
    {
        if (voiceSource == null) return;

        KillFadeTween();

        float startVolume = voiceSource.volume;
        _fadeTween = DOTween.To(
                () => voiceSource.volume,
                v => voiceSource.volume = v,
                0f,
                duration
            )
            .SetEase(fadeEase)
            .SetTarget(gameObject)
            .OnComplete(() =>
            {
                if (stopAfterFade && voiceSource != null)
                    voiceSource.Stop();

                onComplete?.Invoke();
            });
    }

    /// <summary> 立刻停止语音（无淡出） </summary>
    public void StopVoiceImmediately()
    {
        KillFadeTween();
        if (voiceSource != null)
        {
            voiceSource.volume = 0f;
            voiceSource.Stop();
        }
    }

    /// <summary> 淡入背景语音 </summary>
    /// <param name="targetVolume">目标音量（默认 1.0）</param>
    /// <param name="duration">淡入时长</param>
    /// <param name="onComplete">完成回调</param>
    public void FadeInVoice(float targetVolume = 1f, float duration = -1f, System.Action onComplete = null)
    {
        if (voiceSource == null) return;

        if (duration < 0) duration = fadeDuration;

        KillFadeTween();

        if (!voiceSource.isPlaying) voiceSource.Play();

        voiceSource.volume = 0f;
        _fadeTween = DOTween.To(
                () => voiceSource.volume,
                v => voiceSource.volume = v,
                targetVolume,
                duration
            )
            .SetEase(fadeEase)
            .SetTarget(gameObject)
            .OnComplete(() => onComplete?.Invoke());
    }

    /// <summary> 获取当前 AudioSource 引用 </summary>
    public AudioSource GetVoiceSource() => voiceSource;

    #endregion

    #region 内部方法

    private void KillFadeTween()
    {
        if (_fadeTween != null && _fadeTween.IsActive())
        {
            _fadeTween.Kill();
            _fadeTween = null;
        }
    }

    #endregion
}
