using StansAssets.Foundation.Patterns;
using UIManaging.Pages.Common.VideoDetails;
using UIManaging.PopupSystem.Popups.PublishSuccess.Navigation;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    internal sealed class PublishSuccessVideoSongPanel: VideoSongPanel
    {
        protected override void MoveNext()
        {
            var args = new PublishSuccessNavigationArgs(PublishSuccessNavigationCommand.VideoInFeed, ContextData);
            
            StaticBus<PublishSuccessNavigationEvent>.Post(new PublishSuccessNavigationEvent(args));
        }
    }
}