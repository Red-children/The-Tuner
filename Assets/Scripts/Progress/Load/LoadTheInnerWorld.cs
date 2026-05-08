using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadTheInnerWorld : MonoBehaviour
{
    [Header("加载面板")]
    [SerializeField] private UIPanelLoading panelLoading;

    [Header("切换音乐")]
    [SerializeField] private BGMData newBGMData;
    [SerializeField] private float fadeTime = 0.8f;

    private PreciseBGMController _bgm;

    void Awake()
    {
        _bgm = FindObjectOfType<PreciseBGMController>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_bgm == null)
        {
            _bgm = FindObjectOfType<PreciseBGMController>();
            if (_bgm == null)
            {
                CompleteLoading();
                return;
            }
        }

        // ✅ 直接强制停止并切换，不使用淡出
        _bgm.StopBGM();
        
        if (newBGMData != null)
        {
            _bgm.SwitchBGM(newBGMData, false);
        }
        
        // 延迟完成 Loading
        DOVirtual.DelayedCall(0.5f, CompleteLoading);
    }

    void CompleteLoading()
    {
        var loading = UIManager.Instance.GetPanel(UIManager.UIConst.Loading) as UIPanelLoading;
        loading?.Complete(false);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}