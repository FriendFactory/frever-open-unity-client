using Common.Abstract;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.RatingFeed
{
    /// <summary>
    /// Represents a progress bar for rating feed. The current implementation is a PoC and might be changed in the future.
    /// </summary>
    internal sealed class RatingFeedProgressBar: BaseContextPanel<RatingFeedProgress>
    {
        [SerializeField] private Image _progressBarImage;

        private float _targetWidth;
        
        protected override void OnInitialized()
        {
            var parentRectTransform = _progressBarImage.transform.parent.GetComponent<RectTransform>();
            _progressBarImage.rectTransform.sizeDelta = new Vector2(0, _progressBarImage.rectTransform.sizeDelta.y);

            _targetWidth = parentRectTransform.rect.width / ContextData.VideoRatings.Count;

            ContextData.ProgressChanged += OnProgressChanged;
            ContextData.Completed += OnProgressCompleted;
        }

        protected override void BeforeCleanUp()
        {
            ContextData.ProgressChanged -= OnProgressChanged;
            ContextData.Completed -= OnProgressCompleted;
        }

        private void OnProgressChanged(int _) => AnimateProgressStep();
        private void OnProgressCompleted() => AnimateProgressStep();

        private void AnimateProgressStep()
        {
            _progressBarImage.rectTransform
                             .DOSizeDelta(new Vector2(_targetWidth, _progressBarImage.rectTransform.sizeDelta.y), 0.5f)
                             .SetRelative(true);
        }
    }
}