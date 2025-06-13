using System;
using Bridge.Models.VideoServer;
using Common;

namespace UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing
{
    public sealed class VideoSharingModel
    {
        public Video Video { get; }
        public string VideoSharingUrl { get; }
        public int RewardedShareLimit { get; }
        public int SoftCurrencyReward { get; }

        public int ShareCount
        {
            get => _shareCount;
            set
            {
                if (_shareCount == value || ShareDailyLimitReached) return;
                
                _shareCount = value;
                SharedOnce = true;
                
                ShareCountChanged?.Invoke(_shareCount);
            }
        }

        public bool ShareDailyLimitReached => ShareCount == RewardedShareLimit;
        public bool SharedOnce { get; private set; }

        public event Action<int> ShareCountChanged;
        
        private int _shareCount;
        
        public VideoSharingModel(Video video, VideoSharingInfo videoSharingInfo)
        {
            Video = video;
            VideoSharingUrl = videoSharingInfo.SharedPlayerUrl;
            RewardedShareLimit = videoSharingInfo.RewardedShareCount;
            SoftCurrencyReward = videoSharingInfo.SoftCurrency;
            _shareCount = videoSharingInfo.CurrentShareCount;
        }
    }
}