using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;
using UnityEngine;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    [RequireComponent(typeof(NotificationStyleBattleResultItemView))]
    public class StyleBattleResultCellView : NotificationCellView
    {
        public override NotificationType Type => NotificationType.BattleResultCompleted;

        public override void Initialize(NotificationItemModel model)
        {
            var battleModel = model as NotificationStyleBattleResultItemModel;
            
            var view = GetComponent<NotificationStyleBattleResultItemView>();
            view.Initialize(battleModel);
        }
    }
}