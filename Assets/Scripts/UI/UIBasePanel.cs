using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class UIBasePanel : MonoBehaviour
{
    private bool _isRemoved = false;
    private string _name;
    private Coroutine _enterCoroutine; // 协程控制

    [Header("Timeline 动画")]
    [SerializeField] protected PlayableDirector playableDirector;

    [Header("进场开始时间")]
    [SerializeField] private float _enterTime = 0f;

    [Header("退场开始时间（进场结束时间）")]
    [SerializeField] private float _exitTime = 1f;

    [Header("退场动画时长")]
    [SerializeField] private float _exitAnimDuration = 1f;


    public virtual void OpenPanel(string name)
    {
        _name = name;
        gameObject.SetActive(true);
        PlayEnterAnimation();
    }

    public virtual void ClosePanel()
    {
        _isRemoved = true;

        if (_enterCoroutine != null)
        {
            StopCoroutine(_enterCoroutine);
            _enterCoroutine = null;
        }

        PlayExitAnimation();
    }

    // 进场动画
    protected virtual void PlayEnterAnimation()
    {
        if (playableDirector == null) return;

        playableDirector.time = _enterTime;
        playableDirector.Play();
        
        _enterCoroutine = StartCoroutine(WaitAndPause(_exitTime));
    }

    // 退场动画
    protected virtual void PlayExitAnimation()
    {
        if (playableDirector == null)
        {
            DestroyImmediate();
            return;
        }

        playableDirector.time = _exitTime;
        playableDirector.Play();
        Invoke(nameof(DestroyImmediate), _exitAnimDuration);
    }

    // 协程：等待 N 秒后暂停
    private IEnumerator WaitAndPause(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (playableDirector != null)
        {
            playableDirector.Pause();
        }
        _enterCoroutine = null;
    }

    private void DestroyImmediate()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}