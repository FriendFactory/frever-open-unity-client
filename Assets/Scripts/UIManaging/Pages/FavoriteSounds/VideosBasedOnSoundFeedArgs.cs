using System;
using System.Threading;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using Extensions;
using Navigation.Args.Feed;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.FavoriteSounds
{
    public class VideosBasedOnSoundFeedArgs: VideosBasedOnTemplateFeedArgs 
    {
        public IPlayableMusic Sound { get; }
        
        public VideosBasedOnSoundFeedArgs(IPlayableMusic sound, VideoManager videoManager, long idOfFirstVideoToShow, int indexOfFirstVideoToShow) : base(videoManager, idOfFirstVideoToShow, indexOfFirstVideoToShow, -1)
        {
            Sound = sound;
        }

        public override string Name => "Based on Sound";

        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount,
            int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            if (takeNextCount == 0) return;
            
            var type = Sound.GetFavoriteSoundType();
            var id = Sound.GetSoundId();
            
            VideoManager.GetVideosForSound(type, id, onSuccess, onFail, videoId, takeNextCount, takePreviousCount, cancellationToken);
        }
        
        public override bool ShouldShowTabs()
        {
            return false;
        }

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }
        
        protected override void OnVideosDownloaded(Video[] videos, Action<Video[]> callback)
        {
            callback?.Invoke(videos);
        }
    }
}