using Modules.VideoSharing;
using Modules.WatermarkManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Feed.Core;
using UIManaging.Pages.Feed.Core.VideoViewTracking;
using UIManaging.Pages.Feed.Ui.Feed;
using Zenject;

namespace Installers
{
    internal static class VideoServicesBinder
    {
        public static void BindVideoServices(this DiContainer container)
        {
            container.Bind<VideoManager>().AsSingle();
            container.Bind<VideoProvider>().AsSingle();
            container.Bind<FeedLoadingQueue>().AsSingle();
            container.Bind<VideoViewTracker>().AsSingle();
            container.Bind<VideoViewSender>().AsSingle();
            container.Bind<VideoViewFileHandler>().AsSingle();
            container.BindInterfacesAndSelfTo<FeaturedFeedVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<FeedNewVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<ForMeFeedVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<FollowingFeedVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<FriendsFeedVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<HashtagVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<TemplateVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<ProfileVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<TaskVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<UserTaskVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<TrendingFeedVideoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<ProfilesProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<SoundVideoLoader>().AsSingle();
            container.Bind<FeedVideoLikesService>().AsSingle();
            container.BindInterfacesAndSelfTo<VideoSharingHelper>().AsSingle();
            container.BindInterfacesAndSelfTo<WatermarkService>().AsSingle();
        }
    }
}