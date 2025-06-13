using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationInvitationAcceptedModel: NotificationItemModel
    {
        public GroupInfo AcceptedBy { get; }
        public bool IsClaimed { get; }
        public int SoftCurrency { get; }

        public NotificationInvitationAcceptedModel(InvitationAcceptedNotification notificationModel) : base(
            notificationModel.Id, notificationModel.AcceptedBy.Id, notificationModel.Timestamp)
        {
            AcceptedBy = notificationModel.AcceptedBy;
            IsClaimed = notificationModel.Reward.IsClaimed;
            SoftCurrency = notificationModel.Reward.SoftCurrency;
        }

        public override NotificationType Type => NotificationType.InvitationAccepted;
        public override bool IsValid() => true;
    }
}