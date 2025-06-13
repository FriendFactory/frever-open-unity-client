using DG.Tweening;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class ShuffleOverlay : MonoBehaviour
    {
        [SerializeField] private float _slideDuration = 0.25f;

        private RectTransform _rectTransform;
        private float _width;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _rectTransform = (RectTransform) transform;
            _width = _rectTransform.rect.width;

            ResetPosition();
        }

        private void OnDisable()
        {
            _rectTransform.DOKill();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show()
        {
            gameObject.SetActive(true);

            _rectTransform.DOKill();
            _rectTransform.DOLocalMoveX(0, _slideDuration).SetEase(Ease.OutSine);
        }

        public void Hide()
        {
            _rectTransform.DOKill();
            _rectTransform.DOLocalMoveX(_width, _slideDuration)
                          .SetDelay(0.25f)
                          .SetEase(Ease.InSine)
                          .OnComplete(OnComplete);

            void OnComplete()
            {
                gameObject.SetActive(false);
                ResetPosition();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ResetPosition()
        {
            _rectTransform.SetLeft(-_width);
            _rectTransform.SetRight(_width);
        }
    }
}