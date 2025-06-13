using DG.Tweening;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews
{
    public class MediaButtonsAnimator : MonoBehaviour
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        [SerializeField] private CanvasGroup _arrowButton;
        [SerializeField] private CanvasGroup _photoButton;
        [SerializeField] private CanvasGroup _videoButton;
        [SerializeField] private CanvasGroup _createButton;
        [Space]
        [SerializeField] private LayoutElement _mediaButtonsElement;
        [SerializeField] private Vector2 _mediaButtonsMinSize;
        [SerializeField] private Vector2 _mediaButtonsMaxSize;
        [Space]
        [SerializeField] private float _mediaButtonsAnimationTime = 0.5f;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------


        [Button]
        public void ShowMediaButtons()
        {
            CancelAllAnimations();

            _mediaButtonsElement.DOMinSize(_mediaButtonsMaxSize, _mediaButtonsAnimationTime);

            _arrowButton.DOFade(0f, _mediaButtonsAnimationTime)
                        .onComplete += () => _arrowButton.SetActive(false);

            _photoButton.SetActive(true);
            _photoButton.DOFade(1f, _mediaButtonsAnimationTime);

            _videoButton.SetActive(true);
            _videoButton.DOFade(1f, _mediaButtonsAnimationTime);
            
            _createButton.SetActive(true);
            _createButton.DOFade(1f, _mediaButtonsAnimationTime);

        }

        [Button]
        public void HideMediaButtons()
        {
            CancelAllAnimations();

            _mediaButtonsElement.DOMinSize(_mediaButtonsMinSize, _mediaButtonsAnimationTime);

            _arrowButton.SetActive(true);
            _arrowButton.DOFade(1f, _mediaButtonsAnimationTime);

            _photoButton.DOFade(0f, _mediaButtonsAnimationTime)
                        .onComplete += () => _photoButton.SetActive(false);

            _videoButton.DOFade(0f, _mediaButtonsAnimationTime)
                        .onComplete += () => _videoButton.SetActive(false);
            
            _createButton.DOFade(0f, _mediaButtonsAnimationTime)
                         .onComplete += () => _createButton.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CancelAllAnimations()
        {
            DOTween.Kill(_mediaButtonsElement);
            DOTween.Kill(_arrowButton);
            DOTween.Kill(_photoButton);
            DOTween.Kill(_videoButton);
        }
    }
}