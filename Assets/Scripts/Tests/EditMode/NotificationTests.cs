using System;
using System.Collections.Generic;
using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using Moq;
using NUnit.Framework;
using UIManaging.Localization;
using UIManaging.Pages.NotificationPage.NotificationSelection;

namespace Tests.EditMode
{
    internal sealed class NotificationTests
    {
        const string TODAY = "Today";
        const string WEEK = "This week";
        const string MONTH = "This month";
        const string OLDER = "Older";

        private INotificationsLocalization _localization;
        
        [SetUp]
        public void SetupLocalization()
        {
            var localization = new Mock<INotificationsLocalization>();
            localization.Setup(x => x.TimePeriodToday).Returns(TODAY);
            localization.Setup(x => x.TimePeriodThisWeek).Returns(WEEK);
            localization.Setup(x => x.TimePeriodThisMonth).Returns(MONTH);
            localization.Setup(x => x.TimePeriodOlder).Returns(OLDER);
            _localization = localization.Object;
        }
        
        [Test]
        [TestCase("2024-12-13 14:30:45", new[]{"2024-12-13 14:30:00", "2024-12-13 01:30:45"}, new long[]{1}, new []{TODAY})]
        [TestCase("2024-12-13 14:30:45", new[]{"2024-12-13 14:30:45", "2024-12-12 01:30:45"}, new long[]{1,2}, new []{TODAY, WEEK})]
        [TestCase("2024-12-13 14:30:45", new[]{"2024-12-13 14:30:45", "2024-12-13 01:30:45", "2024-12-10 01:30:45"}, new long[]{1,3}, new []{TODAY, WEEK})]
        [TestCase("2024-12-13 14:30:45", new[]{"2024-12-13 14:30:45", "2024-12-13 01:30:45", "2024-12-10 01:30:45", "2024-12-06 01:30:45"}, new long[]{1,3,4}, new []{TODAY, WEEK, MONTH})]
        [TestCase("2024-12-13 14:30:45", new[]{"2024-12-13 14:30:45", "2024-12-13 01:30:30", "2024-12-13 01:30:00", "2024-12-13 00:30:45"}, new long[]{1}, new []{TODAY})]
        [TestCase("2024-12-13 14:30:45", new[]{"2024-12-13 14:30:45", "2024-12-13 01:30:30", "2024-12-13 01:30:00", "2023-12-06 00:30:45"}, new long[]{1,4}, new []{TODAY, OLDER})]
        public void GetNotificationTimeDivider_ShouldProvideCorrectDividers(string currentTimeString, string[] notificationDateTimes, long[] expectedIds, string[] expectedNames)
        {
            //Arrange
            var notificationsHelper = new NotificationListHelper();

            var models = new List<NotificationItemModel>();
            for (var i = 0; i < notificationDateTimes.Length; i++)
            {
                var id = i + 1;
                var model = new Mock<NotificationItemModel>(id, -1, DateTime.Parse(notificationDateTimes[i]));
                models.Add(model.Object);
            }

            var currentTime = DateTime.Parse(currentTimeString);
            
            //Act
            var result = notificationsHelper.GetNotificationTimePeriodSeparatorModel(models.ToArray(), _localization, currentTime);

            //Assert
            Assert.AreEqual(expectedIds.Length, result.Count);
            
            for (var i = 0; i < expectedIds.Length; i++)
            {
                var expectedId = expectedIds[i];
                var actualId = result[i].PlaceBeforeNotificationId;
                Assert.AreEqual(expectedId, actualId);
                var name = expectedNames[i];
                Assert.AreEqual(name,  result[i].Model.Text);
            }
        }

        [Test]
        [TestCase(3, "0:10",new[]{"2024-12-13 14:30:00", "2024-12-13 01:30:45"}, 0)]
        [TestCase(3, "0:10",new[]{"2024-12-13 14:30:00", "2024-12-13 14:25:00", "2024-12-13 14:22:00"}, 1)]
        [TestCase(3, "0:10",new[]{"2024-12-13 14:30:00", "2024-12-13 14:22:00", "2024-12-13 14:13:00"}, 1)]
        [TestCase(3, "0:10",new[]{"2024-12-13 14:30:00", "2024-12-13 14:22:00", "2024-12-13 14:11:00"}, 0)]
        [TestCase(3, "0:10",new[]{"2024-12-13 14:30:00", "2024-12-13 14:22:00", "2024-12-13 14:13:00", "2024-12-13 14:00:00", "2024-12-13 13:00:00", "2024-12-13 12:51:00", "2024-12-13 12:42:00"}, 2)]
        public void SplitNotificationsByGroups(int minGroupSize, string groupingThreshold, string[] notificationDateTimes, int expectedGroupCount)
        {
            //Arrange
            var threshold = TimeSpan.Parse(groupingThreshold);
            var notificationsHelper = new NotificationListHelper();
            var notifications = new NotificationItemModel[notificationDateTimes.Length];
            for (var i = 0; i < notificationDateTimes.Length; i++)
            {
                var id = i + 1;
                var model = new Mock<NotificationItemModel>(id, -1, DateTime.Parse(notificationDateTimes[i]));
                model.Setup(x => x.Type).Returns(NotificationType.NewLikeOnVideo);
                notifications[i] = model.Object;
            }
            
            //Act
            var groups = notificationsHelper.SelectNotificationsThatShouldBeGrouped(notifications, minGroupSize, threshold, NotificationType.NewLikeOnVideo);
            
            //Assert
            Assert.AreEqual(expectedGroupCount, groups.Count);
        }
    }
}