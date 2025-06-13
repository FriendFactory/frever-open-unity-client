using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace Modules.SocialActions
{
    public class LikeVideoAction : ISocialAction
    {
        private readonly PageManager _pageManager;
        private readonly VideoManager _videoManager;
        private readonly long _friendGroupId;
        private readonly long _videoId;

        public LikeVideoAction(long friendGroupId, long videoId, PageManager pageManager, VideoManager videoManager)
        {
            _friendGroupId = friendGroupId;
            _videoId = videoId;

            _pageManager = pageManager;
            _videoManager = videoManager;
        }
        
        public void Execute()
        {
            var args = new RemoteUserFeedArgs(_videoManager, _friendGroupId, _videoId);
            _pageManager.MoveNext(args);
        }
    }
}