using System;
using System.Collections.Generic;
using Extensions;
using Modules.Amplitude;
using Modules.Amplitude.Events.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.Feed.Events.VideoViews
{
    [Serializable]
    internal sealed class VideoViewsAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => AmplitudeEventConstants.EventNames.VIDEO_VIEWS;

        public VideoViewsAmplitudeEvent() { }

        public VideoViewsAmplitudeEvent(IEnumerable<VideoViewData> videoViews, string feedType, VideoListType feedTab)
        {
            var ids = new List<long>();
            var timestamps = new List<string>();

            videoViews.ForEach(swipe =>
            {
                ids.Add(swipe.VideoId);
                timestamps.Add(swipe.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            });
            
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.VIDEO_ID, ids);
            _eventProperties.Add("Timestamp", timestamps);
            _eventProperties.Add("Feed Type", feedType);
            _eventProperties.Add("Feed Tab", feedTab.ToString());
        }
    }
}