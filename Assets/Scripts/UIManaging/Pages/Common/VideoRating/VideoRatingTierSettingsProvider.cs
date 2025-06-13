using System.Collections.Generic;
using JetBrains.Annotations;

namespace UIManaging.Pages.Common.VideoRating
{
    [UsedImplicitly]
    public sealed class VideoRatingTierSettingsProvider
    {
        private readonly Dictionary<VideoRatingRewardTier, VideoRatingTierBadgeSettings> _settings = new();
        
        public VideoRatingTierSettingsProvider(IEnumerable<VideoRatingTierBadgeSettings> settings)
        {
            foreach (var setting in settings)
            {
                _settings.Add(setting.Tier, setting);
            }
        }
        
        public VideoRatingTierBadgeSettings GetSettings(VideoRatingRewardTier tier) => _settings[tier];
    }
}