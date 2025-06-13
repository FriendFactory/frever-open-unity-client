using System;
using Modules.Notifications.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public sealed class NotificationSeasonLikesItemView : NotificationItemView<NotificationSeasonLikesItemModel>, INotificationSource
    {
        public event Action NotificationReceived;
        public long? NotificationId { get; private set; }
        public long? QuestId { get; private set; }
        public bool ShouldRead => false;

        protected override string Description => LikesText();

        protected override void OnInitialized()
        {
            base.OnInitialized();

            NotificationId = ContextData.Id;
            QuestId = ContextData.QuestId;
            NotificationReceived?.Invoke();
        }

        private string LikesText()
        {
            return ContextData.Likes == 0
                ? _localization.SeasonZeroLikesFormat
                : string.Format(_localization.SeasonLikesFormat, ContextData.Likes);
        }
    }
}
