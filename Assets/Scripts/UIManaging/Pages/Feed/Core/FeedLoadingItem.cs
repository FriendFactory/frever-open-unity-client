using Modules.VideoStreaming.Feed;

namespace UIManaging.Pages.Feed.Core
{
    internal class FeedLoadingItem
    {
        public bool AutoPlay { get; }
        public FeedVideoView View { get; }
        public FeedVideoModel Model { get; }

        public FeedLoadingItem(FeedVideoView view, FeedVideoModel model, bool autoPlay = false)
        {
            View = view;
            Model = model;
            AutoPlay = autoPlay;
            view.Initialize(model);
        }
    }
}