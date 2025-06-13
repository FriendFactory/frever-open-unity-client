using System;
using Bridge.Models.VideoServer;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.PopupSystem.Popups.VideoRatingReward
{
    public sealed class VideoRatingRewardPopupConfiguration : PopupConfiguration
    {
        public long VideoId { get; }
        public RatingResult RatingResult { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VideoRatingRewardPopupConfiguration(long videoId, RatingResult ratingResult, Action<object> onClose = null)
            : base(PopupType.VideoRatingRewardPopup, onClose)
        {
            VideoId = videoId;
            RatingResult = ratingResult;
        }
    }
}