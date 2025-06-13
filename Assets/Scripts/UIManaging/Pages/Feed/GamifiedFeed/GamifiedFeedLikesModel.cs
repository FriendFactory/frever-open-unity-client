using System;
using Bridge.Models.VideoServer;

namespace UIManaging.Pages.Feed.GamifiedFeed
{
    public class GamifiedFeedLikesModel: IVideoKPICount
    {
        private readonly Video _video;

        public long Count
        {
            get => _video.KPI.Likes;
            set
            {
                if (_video.KPI.Likes == value) return;
                
                _video.KPI.Likes = value;
                
                Changed?.Invoke(value);
            }
        }

        public bool LikedByCurrentUser
        {
            get => _video.LikedByCurrentUser;
            set => _video.LikedByCurrentUser = value;
        }

        public bool IsOwner { get; }

        public event Action<long> Changed;
        
        public GamifiedFeedLikesModel(Video video, bool isOwner)
        {
            IsOwner = isOwner;
            _video = video;
        }
    }
}