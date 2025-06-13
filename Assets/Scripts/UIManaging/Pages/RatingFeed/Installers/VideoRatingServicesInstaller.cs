using UIManaging.Pages.RatingFeed.Amplitude;
using UIManaging.Pages.RatingFeed.Tracking;
using Zenject;

namespace UIManaging.Pages.RatingFeed
{
    internal sealed class VideoRatingServicesInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VideoRatingAmplitudeEventsFileHandler>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<RatingVideoViewsFileHandler>().AsSingle();
        }
    }
}