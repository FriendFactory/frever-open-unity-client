using System;
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
    public sealed class UserTaskVideoListLoader : LocalUserVideoListLoaderBase
    {
        private event Action<BaseLevelItemArgs> VideoSelected;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public UserTaskVideoListLoader(VideoManager videoManager,
            PageManager pageManager, IBridge bridge, Action<BaseLevelItemArgs> onVideoSelected) : base(videoManager, pageManager, bridge, bridge.Profile.GroupId)
        {
            VideoSelected += onVideoSelected;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override Task<Video[]> DownloadModelsInternal(object targetVideo, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            return VideoManager.GetUserVideoForTasks((long?) targetVideo, UserGroupId, takeNext, takePrevious, token);
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            VideoSelected?.Invoke(args);
        }
    }
}