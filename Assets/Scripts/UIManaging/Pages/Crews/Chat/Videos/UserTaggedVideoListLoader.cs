using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    public sealed class UserTaggedVideoListLoader : LocalUserVideoListLoaderBase
    {
        private event Action<BaseLevelItemArgs> VideoSelected;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public UserTaggedVideoListLoader(VideoManager videoManager,
            PageManager pageManager, IBridge bridge, Action<BaseLevelItemArgs> onVideoSelected) : base(videoManager, pageManager, bridge, bridge.Profile.GroupId)
        {
            VideoSelected += onVideoSelected;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async Task<Video[]> DownloadModelsInternal(object targetVideo, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var result = await Bridge.GetUserTaggedVideoListAsync(UserGroupId, (long?) targetVideo, takeNext, takePrevious, token);

            if (result.IsSuccess) return result.Models;

            if (result.IsError)
            {
                Debug.LogError($"Cannot fetch tagged videos for user {UserGroupId}, target video {targetVideo}.\n" +
                               $"Reason: {result.ErrorMessage}");
            }

            return null;
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            VideoSelected?.Invoke(args);
        }
    }
}