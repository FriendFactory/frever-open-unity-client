using System;
using I2.Loc;
using UIManaging.Pages.Common.VideoRating;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/RatingFeedPageLocalization", fileName = "RatingFeedPageLocalization")]
    public class RatingFeedPageLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _skipRatingDialogTitle;
        [SerializeField] private LocalizedString _skipRatingDialogDesc;
        [SerializeField] private LocalizedString _skipRatingDialogYesButton;
        [SerializeField] private LocalizedString _skipRatingDialogNoButton;
        [SerializeField] private LocalizedString _videRatingRewardClaimButton;
        [SerializeField] private LocalizedString _videoRatingTierBronze;
        [SerializeField] private LocalizedString _videoRatingTierSilver;
        [SerializeField] private LocalizedString _videoRatingTierGold;

        public string SkipRatingDialogTitle => _skipRatingDialogTitle;
        public string SkipRatingDialogDesc => _skipRatingDialogDesc;
        public string SkipRatingDialogYesButton => _skipRatingDialogYesButton;
        public string SkipRatingDialogNoButton => _skipRatingDialogNoButton;
        public string VideRatingRewardClaimButton => _videRatingRewardClaimButton;

        public string GetVideoRatingTier(VideoRatingRewardTier rewardTier)
        {
            return rewardTier switch
            {
                VideoRatingRewardTier.Bronze => _videoRatingTierBronze,
                VideoRatingRewardTier.Silver => _videoRatingTierSilver,
                VideoRatingRewardTier.Gold => _videoRatingTierGold,
                _ => throw new ArgumentOutOfRangeException(nameof(rewardTier), rewardTier, null)
            };
        }
    }
}