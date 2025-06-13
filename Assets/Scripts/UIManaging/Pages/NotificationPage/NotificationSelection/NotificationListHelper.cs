using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Common;
using UIManaging.Localization;

namespace UIManaging.Pages.NotificationPage.NotificationSelection
{
    internal sealed class NotificationListHelper
    {
        private const int DAYS_WITHIN_WEEK = 7;
        private const int DAYS_WITHIN_MONTH = 30;
        
        public List<TimeSeparatorData> GetNotificationTimePeriodSeparatorModel(NotificationItemModel[] notifications, INotificationsLocalization localization, DateTime currentTime)
        {
            var output = new List<TimeSeparatorData>();
            
            var index = 0;
            var notificationItemModel = notifications[index];
            
            if (notificationItemModel.TimeStamp.Year == currentTime.Year
             && currentTime.DayOfYear == notificationItemModel.TimeStamp.DayOfYear)
            {
                output.Add(new TimeSeparatorData
                {
                    PlaceBeforeNotificationId = notificationItemModel.Id,
                    Model = new NotificationTimePeriodSeparatorModel(localization.TimePeriodToday)
                });
                ++index;
                
                for (; index < notifications.Length; ++index)
                {
                    notificationItemModel = notifications[index];
                    
                    if (notificationItemModel.TimeStamp.Year != currentTime.Year || 
                        currentTime.DayOfYear != notificationItemModel.TimeStamp.DayOfYear)
                    {
                        break;
                    }
                }
            }

            if (index == notifications.Length) return output;

            notificationItemModel = notifications[index];
            
            if (GetTimeSpentSincePosted(notificationItemModel).TotalDays < DAYS_WITHIN_WEEK)
            {
                output.Add(new TimeSeparatorData
                {
                    PlaceBeforeNotificationId = notificationItemModel.Id,
                    Model = new NotificationTimePeriodSeparatorModel(localization.TimePeriodThisWeek)
                });
                ++index;
                
                for (; index < notifications.Length; ++index)
                {
                    notificationItemModel = notifications[index];
                    if (GetTimeSpentSincePosted(notificationItemModel).TotalDays >= DAYS_WITHIN_WEEK)
                    {
                        break;
                    }
                }
            }

            if (index == notifications.Length) return output;
            
            notificationItemModel = notifications[index];
           
            if (GetTimeSpentSincePosted(notificationItemModel).TotalDays < DAYS_WITHIN_MONTH)
            {
                output.Add(new TimeSeparatorData
                {
                    PlaceBeforeNotificationId = notificationItemModel.Id,
                    Model = new NotificationTimePeriodSeparatorModel(localization.TimePeriodThisMonth)
                });
                ++index;
                
                for (; index < notifications.Length; ++index)
                {
                    notificationItemModel = notifications[index];
                    if (GetTimeSpentSincePosted(notificationItemModel).TotalDays >= DAYS_WITHIN_MONTH)
                    {
                        break;
                    }
                }
            }
            
            if (index == notifications.Length) return output;
            
            output.Add(new TimeSeparatorData
            {
                PlaceBeforeNotificationId = notifications[index].Id,
                Model = new NotificationTimePeriodSeparatorModel(localization.TimePeriodOlder)
            });
            return output;

            TimeSpan GetTimeSpentSincePosted(UserTimestampItemModel model)
            {
                return currentTime - model.TimeStamp;
            }
        }

        public List<NotificationGroup> SelectNotificationsThatShouldBeGrouped(NotificationItemModel[] notifications, int groupSizeMin, TimeSpan groupNotificationThreshold, NotificationType notificationType)
        {
            var groups = new List<NotificationGroup>();
            
            var targetNotifications = notifications
               .Where(x => x.Type == notificationType);
            var groupedByUserId = targetNotifications.GroupBy(x => x.GroupId);
            
            var notificationsShouldBrGrouped = new List<NotificationItemModel>();
            foreach (var userIdAndNotification in groupedByUserId)
            {
                notificationsShouldBrGrouped.Clear();
                foreach (var notification in userIdAndNotification)
                {
                    if (notificationsShouldBrGrouped.Count == 0)
                    {
                        notificationsShouldBrGrouped.Add(notification);
                        continue;
                    }

                    var previous = notificationsShouldBrGrouped.Last();
                    var difference = (notification.TimeStamp - previous.TimeStamp).Duration();
                    if (difference <= groupNotificationThreshold)
                    {
                        notificationsShouldBrGrouped.Add(notification);
                        continue;
                    }

                    if (notificationsShouldBrGrouped.Count < groupSizeMin)
                    {
                        notificationsShouldBrGrouped.Clear();
                        continue;
                    }
                    
                    groups.Add(CreateNotificationGroup(notificationsShouldBrGrouped));
                    notificationsShouldBrGrouped.Clear();
                }

                if (notificationsShouldBrGrouped.Count >= groupSizeMin)
                {
                    groups.Add(CreateNotificationGroup(notificationsShouldBrGrouped));
                }
            }

            notificationsShouldBrGrouped.Clear();
            return groups;

            NotificationGroup CreateNotificationGroup(IReadOnlyCollection<NotificationItemModel> groupedModels)
            {
                var firstNotification = groupedModels.First();
                return new NotificationGroup
                {
                    Models = groupedModels.ToArray(),
                    FirstNotificationId = firstNotification.Id,
                    UserGroupId = firstNotification.GroupId,
                    DateTime = firstNotification.TimeStamp
                };
            }
        }
    }

    internal sealed class TimeSeparatorData
    {
        public long PlaceBeforeNotificationId;
        public NotificationTimePeriodSeparatorModel Model;
    }
}