using DG.Tweening;
using TMPro;
using TweenExtensions;
using UnityEngine;
using UnityEngine.UI;

public class CreatePostPageButtonAnimator : MonoBehaviour
{
    [SerializeField] private Image _titleImage;
    [SerializeField] private RectTransform _titleImageRect;
    [SerializeField] private RectTransform _titleTextRect;
    [SerializeField] private TMP_Text _tipText;
    [SerializeField] private Image _iconBackground;

    private Sequence _expandSequence;
    
    public void PlayAnimation(bool state, bool instant)
    {
        _expandSequence.PlayByState(state, instant);
    }

    public void InitAnimation(float animationDuration)
    {
        _expandSequence = DOTween.Sequence()
                                 .Join(_titleImageRect.DOSizeDelta(new Vector2(0, -120), animationDuration))
                                 .Join(_titleImageRect.DOAnchorPos(new Vector2(0, 120), animationDuration))
                                 .Join(_titleTextRect.DOAnchorPos(new Vector2(0, 75), animationDuration))
                                 .Join(_titleImage.DOFade(1f, animationDuration))
                                 .Join(_tipText.DOFade(1f, animationDuration))
                                 .Join(_iconBackground.DOFade(1f, animationDuration))
                                 .SetEase(Ease.OutQuad)
                                 .Pause()
                                 .SetAutoKill(false);
    }
}
