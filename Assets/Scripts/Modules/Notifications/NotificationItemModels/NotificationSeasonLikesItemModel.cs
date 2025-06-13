using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationSeasonLikesItemModel : NotificationItemModel
    {
        public int Likes { get; }
        public long QuestId { get; }

        public override NotificationType Type => NotificationType.SeasonQuestAccomplished;
        
        public NotificationSeasonLikesItemModel(long notificationId, long questId, long groupId, DateTime utcTimestamp, int likes) : base(notificationId, groupId, utcTimestamp)
        {
            Likes = likes;
            QuestId = questId;
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
