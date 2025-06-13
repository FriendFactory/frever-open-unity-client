using Modules.Amplitude;
using UIManaging.Pages.Feed.Core;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Feed.Events.VideoViews
{
    internal sealed class VideoViewsAmplitudeEventInstaller: MonoInstaller
    {
        [SerializeField] private FeedPage _feedPage;
        [SerializeField] private FeedManagerView _feedManagerView;
        [Header("Gamified Feed")]
        [SerializeField] private FeedPage _gamifiedFeedPage;
        [SerializeField] private FeedManagerView _gamifiedFeedManagerView;
        
        public override void InstallBindings()
        {
            var feedPage = AmplitudeManager.IsGamifiedFeedEnabled() ? _gamifiedFeedPage : _feedPage;
            var feedManager = AmplitudeManager.IsGamifiedFeedEnabled() ? _gamifiedFeedManagerView : _feedManagerView;
            
            Container.BindInterfacesAndSelfTo<VideoViewsSendEventEmitter>().AsSingle().WithArguments(feedPage, feedManager);
            Container.BindInterfacesAndSelfTo<VideoViewsAmplitudeEventSignalEmitter>().AsSingle().WithArguments(feedManager);
        }
    }
}