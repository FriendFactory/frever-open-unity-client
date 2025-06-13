using UnityEngine.Events;

namespace UIManaging.Pages.Feed.Core
{
    internal sealed class FeedAddCommentPanelModel
    {
        public bool CommentsEnabled { get; }
        public UnityAction OnClicked { get; }

        internal FeedAddCommentPanelModel(bool commentsEnabled, UnityAction onClicked)
        {
            CommentsEnabled = commentsEnabled;
            OnClicked = onClicked;
        }
    }
}