using Common.Abstract;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.VideoRating;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.VideoRatingReward
{
    internal sealed class VideoRatingRewardTierPanel : BaseContextPanel<VideoRatingTierModel>
    {
        [SerializeField] private TMP_Text _ratingText;
        [SerializeField] private VideoRatingTierBadge _tierBadge;
        [SerializeField] private VideoRatingRewardResultBar _resultBar;

        [Inject] private RatingFeedPageLocalization _localization;
        
        protected override void OnInitialized()
        {
            _ratingText.text = _localization.GetVideoRatingTier(ContextData.Tier);
            
            _tierBadge.Initialize(ContextData);
            _resultBar.Initialize(ContextData);
        }
        
        protected override void BeforeCleanUp()
        {
            _tierBadge.CleanUp();
            _resultBar.CleanUp();
        }
    }
}