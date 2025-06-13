using System;
using UIManaging.SnackBarSystem.Configurations;

namespace UIManaging.SnackBarSystem.SnackBars
{
    public class VideoSharedToChatSnackbarConfiguration : SnackBarConfiguration
    {
        public long CrewId;
        public Action OnViewButton;
        internal override SnackBarType Type => SnackBarType.VideoSharedToChat;
    }
}