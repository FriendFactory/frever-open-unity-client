using Modules.Notifications.NotificationItemModels;
using Navigation.Args.Feed;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationCommentOnVideoItemView : NotificationVideoItemView<NotificationCommentOnVideoItemModel>
    {
        protected override string Description => _localization.CommentOnYourVideoFormat;
        
        protected override NotificationFeedArgs GetVideoArgs()
        {
            return new NotificationFeedArgs(VideoManager, PageManager, ContextData.VideoId, ContextData.CommentInfo);
        }
    }
}
