using Modules.Notifications.NotificationItemModels;
using Navigation.Args.Feed;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationNewMentionInCommentOnVideoView :  NotificationVideoItemView<NotificationNewMentionInCommentOnVideoItemModel>
    {
        protected override string Description => _localization.NewMentionInCommentOnVideoFormat;
        
        protected override NotificationFeedArgs GetVideoArgs()
        {
            return new NotificationFeedArgs(VideoManager, PageManager, ContextData.VideoId, ContextData.CommentInfo);
        }
    }
}