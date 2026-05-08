using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class UIRankInfo : MonoBehaviour
{
    [System.Serializable]
    struct RankImages
    {
        public Image miss;
        public Image good;
        public Image great;
        public Image perfect;
    }

    [SerializeField] private RankImages rankImages;
    
    [Header("动画参数")]
    [SerializeField] private float scaleDuration = 0.2f;
    [SerializeField] private float fadeDuration = 0.15f;
    
    private Dictionary<RhythmRank, Image> _rankImageMap;
    private Dictionary<RhythmRank, Sequence> _activeSequences;
    
    #region 初始化
    private void Awake()
    {
        InitRankMap();
        InitAllImages();
        EventBus.Instance.Subscribe<ComboChangedEvent>(OnComboChanged);
        EventBus.Instance.Subscribe<ComboBreakEvent>(OnComboBreak);
    }
    
    private void InitRankMap()
    {
        _rankImageMap = new Dictionary<RhythmRank, Image>();
        _activeSequences = new Dictionary<RhythmRank, Sequence>();
        
        if (rankImages.miss != null)
            _rankImageMap[RhythmRank.Miss] = rankImages.miss;
        if (rankImages.good != null)
            _rankImageMap[RhythmRank.Good] = rankImages.good;
        if (rankImages.great != null)
            _rankImageMap[RhythmRank.Great] = rankImages.great;
        if (rankImages.perfect != null)
            _rankImageMap[RhythmRank.Perfect] = rankImages.perfect;
    }
    
    private void InitAllImages()
    {
        // 初始状态全部隐藏
        foreach (var pair in _rankImageMap)
        {
            if (pair.Value != null)
            {
                pair.Value.gameObject.SetActive(false);
                pair.Value.transform.localScale = Vector3.one;
            }
        }
    }
    #endregion
    
    #region 动画播放
    /// <summary>
    /// 根据判定等级播放对应的艺术字动画
    /// </summary>
    public void PlayRankAnimation(RhythmRank rank)
    {
        if (!_rankImageMap.TryGetValue(rank, out Image targetImage))
        {
            Debug.LogWarning($"未找到等级 {rank} 对应的艺术字图片");
            return;
        }
        
        if (targetImage == null) return;
        
        // 停止当前正在播放的相同等级动画
        if (_activeSequences.ContainsKey(rank))
        {
            _activeSequences[rank]?.Kill();
            _activeSequences.Remove(rank);
        }
        
        // 重置图片状态
        targetImage.transform.localScale = Vector3.one;
        targetImage.gameObject.SetActive(true);
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 根据等级选择不同的动画效果
        switch (rank)
        {
            case RhythmRank.Miss:
                sequence = BuildMissAnimation(targetImage);
                break;
            case RhythmRank.Good:
                sequence = BuildGoodAnimation(targetImage);
                break;
            case RhythmRank.Great:
                sequence = BuildGreatAnimation(targetImage);
                break;
            case RhythmRank.Perfect:
                sequence = BuildPerfectAnimation(targetImage);
                break;
            default:
                sequence = BuildDefaultAnimation(targetImage);
                break;
        }
        
        // 动画结束后隐藏
        sequence.OnComplete(() =>
        {
            if (targetImage != null)
            {
                targetImage.gameObject.SetActive(false);
                targetImage.transform.localScale = Vector3.one;
            }
            _activeSequences.Remove(rank);
        });
        
        // 存储序列引用
        _activeSequences[rank] = sequence;
        sequence.Play();
    }
    
    /// <summary>
    /// Miss 动画：缩小
    /// </summary>
    private Sequence BuildMissAnimation(Image target)
    {
        Sequence seq = DOTween.Sequence();
        
        seq.Append(target.transform.DOScale(1.2f, scaleDuration * 0.5f).SetEase(Ease.OutBack));
        seq.Append(target.transform.DOScale(0.5f, scaleDuration * 0.5f).SetEase(Ease.InBack));
        
        return seq;
    }
    
    /// <summary>
    /// Good 动画：弹跳放大
    /// </summary>
    private Sequence BuildGoodAnimation(Image target)
    {
        Sequence seq = DOTween.Sequence();
        
        seq.Append(target.transform.DOPunchScale(Vector3.one * 0.8f, scaleDuration, vibrato: 1, elasticity: 1));
        
        return seq;
    }
    
    /// <summary>
    /// Great 动画：放大 + 震动
    /// </summary>
    private Sequence BuildGreatAnimation(Image target)
    {
        Sequence seq = DOTween.Sequence();
        
        seq.Append(target.transform.DOScale(1.3f, scaleDuration).SetEase(Ease.OutElastic));
        seq.Join(target.transform.DOShakePosition(0.2f, 3f, 5, 45));
        
        return seq;
    }
    
    /// <summary>
    /// Perfect 动画：放大 + 脉冲 + 旋转
    /// </summary>
    private Sequence BuildPerfectAnimation(Image target)
    {
        Sequence seq = DOTween.Sequence();
        
        // 放大效果
        seq.Append(target.transform.DOScale(1.5f, scaleDuration).SetEase(Ease.OutBack));
        
        // 脉冲效果（快速缩放循环）
        seq.Append(target.transform.DOScale(1.3f, 0.1f).SetEase(Ease.OutQuad));
        seq.Append(target.transform.DOScale(1.5f, 0.1f).SetEase(Ease.InQuad));
        seq.Append(target.transform.DOScale(1.4f, 0.05f));
        
        // 轻微旋转
        seq.Join(target.transform.DORotate(new Vector3(0, 0, 15f), 0.1f, RotateMode.FastBeyond360));
        seq.Join(target.transform.DORotate(new Vector3(0, 0, -15f), 0.1f, RotateMode.FastBeyond360));
        seq.Join(target.transform.DORotate(new Vector3(0, 0, 0f), 0.1f, RotateMode.FastBeyond360));
        
        return seq;
    }
    
    /// <summary>
    /// 默认动画
    /// </summary>
    private Sequence BuildDefaultAnimation(Image target)
    {
        Sequence seq = DOTween.Sequence();
        
        seq.Append(target.transform.DOScale(1.3f, scaleDuration).SetEase(Ease.OutBack));
        
        return seq;
    }
    #endregion
    
    #region 对外接口
    /// <summary>
    /// 通过事件触发动画
    /// </summary>
    public void OnEnemyHit(RhythmRank rank)
    {
        PlayRankAnimation(rank);
    }
    public void OnEnemyHit(EnemyHitEvent evt)
    {
        OnEnemyHit(evt.rank);
    }
    public void OnComboChanged(ComboChangedEvent evt)
    {
        PlayRankAnimation(evt.rank);
    }
    public void OnComboBreak(ComboBreakEvent evt)
    {
        PlayRankAnimation(evt.rank);
    }
    /// <summary>
    /// 停止所有正在播放的动画
    /// </summary>
    public void StopAllAnimations()
    {
        foreach (var seq in _activeSequences.Values)
        {
            seq?.Kill();
        }
        _activeSequences.Clear();
        
        // 隐藏所有图片并重置缩放
        foreach (var pair in _rankImageMap)
        {
            if (pair.Value != null)
            {
                pair.Value.gameObject.SetActive(false);
                pair.Value.transform.localScale = Vector3.one;
            }
        }
    }
    #endregion
    
    #region 生命周期
    private void OnDestroy()
    {
        StopAllAnimations();
    }
    #endregion
}