using StansAssets.Foundation.Patterns;

namespace UIManaging.PopupSystem.Popups.PublishSuccess.Navigation
{
    public class PublishSuccessNavigationEvent : IEvent
    {
        public PublishSuccessNavigationArgs NavigationArgs { get; }
        
        public PublishSuccessNavigationEvent(PublishSuccessNavigationArgs navigationArgs)
        {
            NavigationArgs = navigationArgs;
        }
    }
}