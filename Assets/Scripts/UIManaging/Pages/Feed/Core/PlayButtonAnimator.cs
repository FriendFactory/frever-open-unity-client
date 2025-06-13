using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TweenExtensions;
using UnityEngine;
using UnityEngine.UI;

public class PlayButtonAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Image _image;
    
    [SerializeField] private Sprite _playIcon;
    [SerializeField] private Sprite _pauseIcon;


    private Sequence _appearSequence;
    
    private void Awake()
    {
        _appearSequence = DOTween.Sequence()
                                 .Join(_canvasGroup.DOFade(1, 0.2f))
                                 .Join(_rectTransform.DOScale(Vector3.one, 0.2f))
                                 .AppendInterval(1f)
                                 .Append(_canvasGroup.DOFade(0, 0.2f))
                                 .Join(_rectTransform.DOScale(Vector3.zero, 0.2f))
                                 .SetAutoKill(false)
                                 .Pause();
    }

    public void PlayAnimation(bool isPlaying)
    {
        _image.sprite = isPlaying ? _playIcon : _pauseIcon;
        _appearSequence.Rewind();
        _appearSequence.PlayByState(true);
    }

    private void OnDestroy()
    {
        _appearSequence.Kill();
    }
}
