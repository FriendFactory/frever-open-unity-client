using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.NotificationServer;
using Common;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Localization;

namespace UIManaging.Pages.NotificationPage.NotificationSelection
{
    public sealed class NotificationListModel
    {
        public readonly List<object> ListItemModels = new();
        private readonly NotificationListHelper _notificationListHelper = new();

        public NotificationListModel(NotificationItemModel[] notifications, INotificationsLocalization localization)
        {
            var timeSeparators = _notificationListHelper.GetNotificationTimePeriodSeparatorModel(notifications, localization, DateTime.UtcNow);
            var groupedNotifications = _notificationListHelper.SelectNotificationsThatShouldBeGrouped(notifications, Constants.Notifications.GROUP_SIZE_MIN, Constants.Notifications.GROUP_NOTIFICATIONS_THRESHOLD, NotificationType.NewLikeOnVideo);
            
            foreach (var notification in notifications)
            {
                var timeSeparator = timeSeparators.FirstOrDefault(x => x.PlaceBeforeNotificationId == notification.Id);
                if (timeSeparator != null)
                {
                    ListItemModels.Add(timeSeparator.Model);
                    timeSeparators.Remove(timeSeparator);
                }

                var group = groupedNotifications.FirstOrDefault(x => x.FirstNotificationId == notification.Id);
                if (group != null)
                {
                    var groupModel = new NotificationsGroupModel(notification.Id, group.UserGroupId, group.DateTime)
                    {
                        GroupedModels = group.Models
                    };
                    ListItemModels.Add(groupModel);
                    continue;
                }
                
                if(groupedNotifications.Any(x => x.Models.Contains(notification))) continue;
                
                ListItemModels.Add(notification);
            }
        }
    }
}
