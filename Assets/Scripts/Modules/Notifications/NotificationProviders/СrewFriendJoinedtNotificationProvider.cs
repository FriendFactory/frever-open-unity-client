using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.Common.UsersManagement;

namespace Modules.Notifications.NotificationProviders
{
    public class СrewFriendJoinedNotificationProvider : BaseNotificationProvider
    {
        private readonly LocalUserDataHolder _dataHolder;

        public СrewFriendJoinedNotificationProvider(LocalUserDataHolder dataHolder)
        {
            _dataHolder = dataHolder;
        }

        public override NotificationType Type => NotificationType.FriendJoinedCrew;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var model = notification as FriendJoinedCrewNotification;

            return new NotificationCrewFriendJoinedModel(model.Friend.Nickname, model.Id, model.Crew.Id, model.Friend.Id,
                                                         model.Timestamp);
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var model = notification as FriendJoinedCrewNotification;

            return model?.Friend != null && model.Crew != null 
                    && _dataHolder.UserProfile.CrewProfile?.Id == model.Crew.Id;
        }
    }
}