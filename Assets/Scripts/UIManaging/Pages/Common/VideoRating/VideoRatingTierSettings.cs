using UnityEngine;

namespace UIManaging.Pages.Common.VideoRating
{
    [CreateAssetMenu(fileName = "VideoRatingTierSettings", menuName = "Friend Factory/UI/Video Rating Tier Settings", order = 1)]
    public sealed class VideoRatingTierBadgeSettings: ScriptableObject
    {
        [SerializeField] private VideoRatingRewardTier _tier;
        [SerializeField] private Color _primaryColor = Color.white;
        [SerializeField] private Color _secondaryColor = Color.gray;

        public Color PrimaryColor => _primaryColor; 
        public Color SecondaryColor => _secondaryColor;
        public VideoRatingRewardTier Tier => _tier;
    }
}