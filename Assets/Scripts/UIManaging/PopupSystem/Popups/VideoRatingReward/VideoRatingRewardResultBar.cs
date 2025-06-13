using Common.Abstract;
using Extensions;
using TMPro;
using UIManaging.Pages.Common.VideoRating;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.VideoRatingReward
{
    internal sealed class VideoRatingRewardResultBar: BaseContextPanel<VideoRatingTierModel>
    {
        private const int MIN_RATING = 10;
        private const int MAX_RATING = 35;
        
        [SerializeField] private Slider _progressBar;
        [Header("Voting Result")]
        [SerializeField] private RectTransform _votingResultTransform;
        [SerializeField] private TMP_Text _votingResult;
        [SerializeField] private float _votingResultOffsetX = 30f;
        
        private RectTransform _progressBarTransform;

        protected override bool IsReinitializable => true;

        private void Awake()
        {
            _progressBarTransform = _progressBar.GetComponent<RectTransform>();
        }

        protected override void OnInitialized()
        {
            _progressBar.value = ContextData.Rating;
            _votingResult.text = ContextData.Rating.ToString();
            _progressBar.value = Mathf.Clamp(ContextData.Rating, MIN_RATING, MAX_RATING);
            
            var progressBarWidth = _progressBarTransform.GetWidth();
           
            _votingResultTransform.SetAnchoredX(_progressBar.normalizedValue * progressBarWidth + _votingResultOffsetX);
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _votingResultTransform.SetAnchoredX(0f);
        }
    }
}