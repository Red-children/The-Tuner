using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPanelGameOver : UIBasePanel
{
    [Header("动画参数")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float scaleDuration = 0.3f;

    [Header("UI组件")]
    [SerializeField] private Image background;
    [SerializeField] private Text titleText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Button buttonRestart;
    [SerializeField] private Button buttonMainMenu;
    [SerializeField] private Button buttonQuit;

    #region 覆写动画
    protected override void PlayEnterAnimation()
    {
        if (_seq != null)
        {
            _seq.Kill();
            _seq = null;
        }
        _seq = DOTween.Sequence();

        _isPlayingAnimation = true;

        // 背景淡入
        _seq.Join(FadeIn(background, fadeDuration));
        
        // 标题缩放淡入
        if (titleText != null)
        {
            _seq.Join(titleText.DOFade(1, fadeDuration).From(0));
            _seq.Join(titleText.rectTransform.DOScale(1, scaleDuration).From(0).SetEase(Ease.OutBounce));
        }

        // 分数淡入
        if (scoreText != null)
        {
            _seq.Join(FadeIn(scoreText, fadeDuration).SetDelay(0.2f));
        }

        // 按钮淡入缩放
        if (buttonRestart != null)
        {
            _seq.Join(FadeIn(buttonRestart.image, fadeDuration).SetDelay(0.3f));
            _seq.Join(ScaleIn(buttonRestart.transform, scaleDuration).SetDelay(0.3f));
        }
        if (buttonMainMenu != null)
        {
            _seq.Join(FadeIn(buttonMainMenu.image, fadeDuration).SetDelay(0.4f));
            _seq.Join(ScaleIn(buttonMainMenu.transform, scaleDuration).SetDelay(0.4f));
        }
        if (buttonQuit != null)
        {
            _seq.Join(FadeIn(buttonQuit.image, fadeDuration).SetDelay(0.5f));
            _seq.Join(ScaleIn(buttonQuit.transform, scaleDuration).SetDelay(0.5f));
        }

        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            TriggerOnOpenComplete();
        });
        _seq.SetUpdate(true);
        _seq.SetTarget(gameObject);
    }

    protected override void PlayExitAnimation(bool destroyAfter)
    {
        _isPlayingAnimation = true;

        _seq = DOTween.Sequence();
        
        // 按钮淡出缩放
        if (buttonRestart != null)
        {
            _seq.Join(FadeOut(buttonRestart.image, fadeDuration));
            _seq.Join(ScaleOut(buttonRestart.transform, scaleDuration));
        }
        if (buttonMainMenu != null)
        {
            _seq.Join(FadeOut(buttonMainMenu.image, fadeDuration));
            _seq.Join(ScaleOut(buttonMainMenu.transform, scaleDuration));
        }
        if (buttonQuit != null)
        {
            _seq.Join(FadeOut(buttonQuit.image, fadeDuration));
            _seq.Join(ScaleOut(buttonQuit.transform, scaleDuration));
        }

        // 分数淡出
        if (scoreText != null)
        {
            _seq.Join(FadeOut(scoreText, fadeDuration));
        }

        // 标题淡出
        if (titleText != null)
        {
            _seq.Join(titleText.DOFade(0, fadeDuration));
            _seq.Join(titleText.rectTransform.DOScale(0, scaleDuration).SetEase(Ease.InBack));
        }

        // 背景淡出
        _seq.Join(FadeOut(background, fadeDuration));

        _seq.OnComplete(() =>
        {
            _isPlayingAnimation = false;
            TriggerOnCloseComplete();
            if (destroyAfter)
                Destroy(gameObject);
            else HideImmediately();
        });
        _seq.SetUpdate(true);
        _seq.SetTarget(gameObject);
    }
    #endregion

    #region 按钮回调
    public void OnClickRestart()
    {
        RegisterOnCloseComplete(() =>
        {
            // 重新加载当前场景
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        UIManager.Instance.ClosePanel(this);
    }

    public void OnClickMainMenu()
    {
        RegisterOnCloseComplete(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
        UIManager.Instance.ClosePanel(this);
    }

    public void OnClickQuit()
    {
        // 关闭游戏
        RegisterOnCloseComplete(() =>
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
        UIManager.Instance.ClosePanel(this);
    }
    #endregion

    #region 生命周期
    protected override void Awake()
    {
        base.Awake();

        if (buttonRestart != null)
            buttonRestart.onClick.AddListener(OnClickRestart);
        if (buttonMainMenu != null)
            buttonMainMenu.onClick.AddListener(OnClickMainMenu);
        if (buttonQuit != null)
            buttonQuit.onClick.AddListener(OnClickQuit);
    }

    /// <summary>
    /// 设置结算分数
    /// </summary>
    public void SetScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"得分: {score}";
        }
    }
    #endregion
}