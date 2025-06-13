using DG.Tweening;
using UnityEngine;

namespace UIManaging.Pages.Loading
{
    public class StartupLoadingAnimator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _loadingBarCanvasGroup;
        [SerializeField] private RectTransform _textRectTransform;
        
        private Sequence _sequence;

        public void Play()
        {
            _sequence = DOTween.Sequence()
                               .SetDelay(0.5f)
                               .Append(_loadingBarCanvasGroup.DOFade(1.0f, 0.5f))
                               .Append(_textRectTransform.DOAnchorPos3D(Vector3.zero, 0.5f));
            _sequence.Play();
        }
    }
}