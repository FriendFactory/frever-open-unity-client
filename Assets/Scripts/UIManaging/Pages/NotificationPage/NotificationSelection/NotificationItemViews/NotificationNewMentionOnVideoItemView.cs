using Modules.Notifications.NotificationItemModels;
using Navigation.Args.Feed;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationNewMentionOnVideoItemView : NotificationVideoItemView<NotificationNewMentionOnVideoItemModel>
    {
        protected override string Description => _localization.NewMentionOnVideoFormat;
        
        protected override NotificationFeedArgs GetVideoArgs()
        {
            return new NotificationFeedArgs(VideoManager, PageManager, ContextData.VideoId, null);
        }
    }
}
