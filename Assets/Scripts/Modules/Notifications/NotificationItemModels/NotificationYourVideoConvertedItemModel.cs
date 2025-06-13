using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationYourVideoConvertedItemModel : NotificationVideoItemModel
    {
        public string VideoUrl { get; }
        
        public NotificationYourVideoConvertedItemModel(long notificationId, long groupId, long videoId, long levelId, string videoUrl, DateTime utcTimestamp, EventInfo eventInfo) 
            : base(notificationId, groupId, videoId, levelId, utcTimestamp, eventInfo)
        {
            VideoUrl = videoUrl;
        }

        public override NotificationType Type => NotificationType.YourVideoConverted;
    }
}