using Modules.Notifications.NotificationItemModels;
using Navigation.Args.Feed;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationCommentOnVideoYouHaveCommentedOnItemView : NotificationVideoItemView<NotificationCommentOnVideoYouHaveCommentedOnItemModel>
    {
        protected override string Description =>
            ContextData.CommentInfo?.ReplyTo?.CommentedBy.Id == Bridge.Profile.GroupId 
                ? _localization.NewMentionInReplyOnVideoFormat 
                : _localization.NewMentionInCommentOnVideoFormat;
        
        protected override NotificationFeedArgs GetVideoArgs()
        {
            return new NotificationFeedArgs(VideoManager, PageManager, ContextData.VideoId, ContextData.CommentInfo);
        }
    }
}
