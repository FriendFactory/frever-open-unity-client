using Bridge;
using Bridge.NotificationServer;
using Modules.AssetsStoraging.Core;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class SeasonLikesNotificationProvider : BaseNotificationProvider
    {
        private readonly IDataFetcher _dataFetcher;
        private readonly IBridge _bridge;
    
        public SeasonLikesNotificationProvider(IDataFetcher dataFetcher, IBridge bridge)
        {
            _bridge = bridge;
            _dataFetcher = dataFetcher;
        }
        
        public override NotificationType Type => NotificationType.SeasonQuestAccomplished;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var seasonLikesNotification = notification as SeasonQuestAccomplishedNotification;

            if (seasonLikesNotification == null) return null;
            
            var itemModel = new NotificationSeasonLikesItemModel(seasonLikesNotification.Id, 
                                                                 seasonLikesNotification.SeasonQuestId, 
                                                                 _bridge.Profile.GroupId, 
                                                                 seasonLikesNotification.Timestamp, 
                                                                 seasonLikesNotification.LikeCount);
            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            return notification is SeasonQuestAccomplishedNotification && _dataFetcher.CurrentSeason != null;
        }
    }
}
