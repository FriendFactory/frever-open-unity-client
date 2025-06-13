using System.Collections.Generic;
using Bridge.VideoServer;
using JetBrains.Annotations;
using Modules.TempSaves.Manager;
using UIManaging.Pages.Feed.Ui.Feed;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Tracking
{
    [UsedImplicitly]
    internal sealed class RatingVideoViewsFileHandler: UnsentDataFileHandler<List<VideoView>>, IInitializable
    {
        private const string FILE_NAME = "UnsentRatingVideoViews.txt";

        private readonly VideoViewSender _videoViewSender;
        
        public RatingVideoViewsFileHandler(TempFileManager tempFileManager, VideoViewSender videoViewSender) : base(tempFileManager, FILE_NAME)
        {
            _videoViewSender = videoViewSender;
        }

        public void Initialize()
        {
            if (TryLoad(out var unsentVideoViews))
            {
                _videoViewSender.Send(unsentVideoViews.ToArray());
            }
        }
    }
}