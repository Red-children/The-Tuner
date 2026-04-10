using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PanelPlayerHurtEffect : UIBasePanel
{
    [SerializeField] private Image image;   //  受伤Image
    private Tweener _currentTween;
    #region 回调函数 
    private void OnPlayerHurt(PlayerHurtEvent evt)
    {
        image.DOKill();
        
        image.DOFade(1f, 0.2f)
             .SetEase(Ease.OutQuad)
             .OnComplete(() =>
             {
                 image.DOFade(0f, 0.8f).SetEase(Ease.OutQuad);
             });
    }
    #endregion
    #region 生命周期
    private void Awake()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if(image == null)
        {
            Debug.LogError("PanelPlayerHurtEffect 未找到组件");
        }
        //  不遮挡
        image.raycastTarget = false;
    }
    private void Start()
    {
        EventBus.Instance.Subscribe<PlayerHurtEvent>(OnPlayerHurt);
    }
    private void Update()
    {
        EasyTest();
    }
    #endregion
    void EasyTest()
    {
        if (Input.GetMouseButtonDown(0))
        {
            EventBus.Instance.Trigger<PlayerHurtEvent>(new PlayerHurtEvent());
        }
    }
}
