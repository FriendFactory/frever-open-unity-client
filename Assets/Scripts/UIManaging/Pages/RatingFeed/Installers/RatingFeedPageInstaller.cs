using UIManaging.Pages.RatingFeed.Amplitude;
using UIManaging.Pages.RatingFeed.Rating;
using UIManaging.Pages.RatingFeed.Signals;
using UIManaging.Pages.RatingFeed.Tracking;
using Zenject;

namespace UIManaging.Pages.RatingFeed
{
    public class RatingFeedPageInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<ScoreAnimationFinishedSignal>();
            Container.DeclareSignal<RatingVideoStartedPlayingSignal>();
            
            Container.BindInterfacesAndSelfTo<RatingFeedPageModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<VideosForRatingListProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<VideoRatingSender>().AsSingle();
            Container.BindInterfacesAndSelfTo<RatingFeedViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<RatingFeedProgress>().AsSingle();
            Container.BindInterfacesAndSelfTo<RatingFeedFollowStatusCache>().AsSingle();

            Container.BindInterfacesAndSelfTo<VideoRatingAmplitudeEventSignalEmitter>().AsSingle();

            Container.BindInterfacesAndSelfTo<RatingVideoViewsTracker>().AsSingle();
        }
    }
}