using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.Crews
{
    public sealed class UserPublicVideoListLoader : LocalUserVideoListLoaderBase
    {
        private event Action<BaseLevelItemArgs> VideoSelected;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public UserPublicVideoListLoader(VideoManager videoManager,
            PageManager pageManager, IBridge bridge, Action<BaseLevelItemArgs> onVideoSelected) : base(videoManager, pageManager, bridge, bridge.Profile.GroupId)
        {
            VideoSelected += onVideoSelected;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async Task<Video[]> DownloadModelsInternal(object targetVideo, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var results = await VideoManager.GetVideosForLocalUser((long?) targetVideo, takeNext, takePrevious, token);
            return results.Video.ToArray();
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            VideoSelected?.Invoke(args);
        }
    }
}