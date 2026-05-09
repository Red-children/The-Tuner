using UnityEngine;

/// <summary>
/// 音效系统调试器 - 检查音效播放的所有环节
/// </summary>
public class AudioDebugger : MonoBehaviour
{
    [Header("手动测试")]
    [Tooltip("点击按钮测试音效播放")]
    public bool playTestSound = false;

    private void Awake()
    {
        Debug.Log("========== Audio Debugger Initialized ==========");
    }

    private void Start()
    {
        CheckAudioSystem();
    }

    private void Update()
    {
        if (playTestSound)
        {
            playTestSound = false;
            PlayTestShootSound();
        }
    }

    [ContextMenu("检查音效系统")]
    private void CheckAudioSystem()
    {
        Debug.Log("\n=== 检查 AudioManager ===");
        
        // 1. 检查 AudioManager 是否存在
        if (AudioManager.Instance == null)
        {
            Debug.LogError("❌ AudioManager.Instance 为 null！");
            return;
        }
        Debug.Log("✓ AudioManager.Instance 存在");

        // 2. 检查 sfxSource 是否赋值
        if (AudioManager.Instance.sfxSource == null)
        {
            Debug.LogError("❌ AudioManager.sfxSource 未赋值！");
        }
        else
        {
            Debug.Log("✓ AudioManager.sfxSource 已赋值");
            Debug.Log($"  └─ AudioSource 名称: {AudioManager.Instance.sfxSource.gameObject.name}");
            Debug.Log($"  └─ 音量: {AudioManager.Instance.sfxSource.volume}");
            Debug.Log($"  └─ 静音: {AudioManager.Instance.sfxSource.mute}");
            Debug.Log($"  └─ 播放中: {AudioManager.Instance.sfxSource.isPlaying}");
        }

        // 3. 检查音效剪辑
        Debug.Log("\n=== 检查音效剪辑 ===");
        CheckClip("shootClip", AudioManager.Instance.shootClip);
        CheckClip("hitClip", AudioManager.Instance.hitClip);
        CheckClip("enemyDeathClip", AudioManager.Instance.enemyDeathClip);

        // 4. 检查事件订阅
        Debug.Log("\n=== 检查事件订阅 ===");
        Debug.Log("提示：无法直接检查 EventBus 订阅状态，请运行游戏并观察日志");

        // 5. 检查 AudioListener
        Debug.Log("\n=== 检查 AudioListener ===");
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        Debug.Log($"场景中有 {listeners.Length} 个 AudioListener");
        foreach (AudioListener listener in listeners)
        {
            Debug.Log($"  └─ {listener.gameObject.name} (启用: {listener.enabled})");
        }

        Debug.Log("\n========== 音效系统检查完成 ==========");
    }

    private void CheckClip(string name, AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError($"❌ {name} 未赋值！");
        }
        else
        {
            Debug.Log($"✓ {name} 已赋值");
            Debug.Log($"  └─ 名称: {clip.name}");
            Debug.Log($"  └─ 时长: {clip.length:F2}秒");
            Debug.Log($"  └─ 采样率: {clip.frequency}Hz");
        }
    }

    [ContextMenu("测试射击音效")]
    private void PlayTestShootSound()
    {
        Debug.Log("\n=== 测试射击音效 ===");
        
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager 不存在");
            return;
        }

        if (AudioManager.Instance.sfxSource == null)
        {
            Debug.LogError("sfxSource 未赋值");
            return;
        }

        if (AudioManager.Instance.shootClip == null)
        {
            Debug.LogError("shootClip 未赋值");
            return;
        }

        Debug.Log("播放测试射击音效...");
        AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.shootClip);
        
        if (AudioManager.Instance.sfxSource.isPlaying)
        {
            Debug.Log("✓ 音效正在播放");
        }
        else
        {
            Debug.LogError("❌ 音效未能播放");
        }
    }
}