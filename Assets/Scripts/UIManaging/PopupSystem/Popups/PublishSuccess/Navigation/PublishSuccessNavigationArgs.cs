using Bridge.Models.VideoServer;

namespace UIManaging.PopupSystem.Popups.PublishSuccess.Navigation
{
    public class PublishSuccessNavigationArgs
    {
        public PublishSuccessNavigationCommand NavigationCommand { get; }
        public Video Video { get; }
 
        public PublishSuccessNavigationArgs(PublishSuccessNavigationCommand navigationCommand, Video video)
        {
            NavigationCommand = navigationCommand;
            Video = video;
        }
    }
}