using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationVideoGotRatingItemModel : NotificationVideoItemModel
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public NotificationVideoGotRatingItemModel(long notificationId, long videoId, long levelId, DateTime utcTimestamp,
            EventInfo ev) : base(notificationId, 0, videoId, levelId, utcTimestamp, ev)
        {
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override NotificationType Type => NotificationType.VideoRatingCompleted;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override bool IsValid()
        {
            return VideoId != 0;
        }
    }
}