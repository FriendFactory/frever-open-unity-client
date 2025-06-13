using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Common.SlideShows
{
    internal sealed class SpriteSlideShow : MonoBehaviour
    {
        [SerializeField] private float _transitionDuration = .5f;
        [SerializeField] private float _slideDuration = 2f;

        [SerializeField] private Sprite[] _slides;

        [SerializeField] private Image _image1;
        [SerializeField] private Image _image2;

        private Image _currentImage; // Visible image with actual sprite
        private Image _nextImage; // Hidden image with next sprite to show after a while

        private int _index; // Index of actual sprite

        private Coroutine _routine;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _currentImage = _image1;
            _nextImage = _image2;
        }

        private void OnEnable()
        {
            ResetSlides();
            RunSlideShow();
        }

        private void OnDisable()
        {
            if (null != _routine)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void ResetSlides()
        {
            if (_slides.Length > 0)
            {
                _index = 0;
                _currentImage.sprite = _slides[_index];

                _currentImage.CrossFadeAlpha(1f, 0f, true); // Instant show
                _nextImage.CrossFadeAlpha(0f, 0f, true); // Instant hide
            }
        }

        private void RunSlideShow()
        {
            const int minSpritesCount = 2;
            if (_slides.Length > minSpritesCount)
            {
                _index++;
                if (_index >= _slides.Length) _index = 0;

                _nextImage.sprite = _slides[_index];
                _routine = StartCoroutine(ShowSlideRoutine());
            }
        }

        private IEnumerator ShowSlideRoutine()
        {
            // Wait some time to display current slide
            yield return new WaitForSeconds(_slideDuration);

            // Transition to next slide
            _currentImage.CrossFadeAlpha(0f, _transitionDuration, true);
            _nextImage.CrossFadeAlpha(1f, _transitionDuration, true);

            // Wait end of slides transition
            yield return new WaitForSeconds(_transitionDuration);

            // Swap
            (_currentImage, _nextImage) = (_nextImage, _currentImage);

            // Go to next slide
            RunSlideShow();
        }
    }
}