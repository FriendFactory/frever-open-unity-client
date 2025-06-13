using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public abstract class NotificationVideoItemModel : NotificationItemModel
    {
        public long VideoId { get; }
        public long LevelId { get; }
        public EventInfo ThumbnailEventInfo { get; }

        protected NotificationVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, EventInfo thumbnailEventInfo) : base(notificationId, groupId, utcTimestamp)
        {
            VideoId = videoId;
            LevelId = levelId;
            ThumbnailEventInfo = thumbnailEventInfo;
        }

        public override bool IsValid()
        {
            return GroupId != 0 && VideoId != 0;
        }
    }
}
