using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UnityEngine;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public sealed class NotificationVideoDeletedItemView : NotificationItemView<NotificationVideoDeletedItemModel>
    {
        [SerializeField] private EventThumbnail _eventThumbnail;

        protected override string Description => _localization.VideoDeletedFormat;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetupThumbnail();
        }

        private void SetupThumbnail()
        {
            _eventThumbnail.Initialize(ContextData.Notification.Event);
        }
    }
}
