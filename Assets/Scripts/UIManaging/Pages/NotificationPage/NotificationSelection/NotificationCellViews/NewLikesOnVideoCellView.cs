using System;
using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using TMPro;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public sealed class NewLikesOnVideoCellView : UserBasedNotificationCellView<VideoLikeNotificationsGroupCellView, NotificationsGroupModel>
    {
        [SerializeField] private Button _showAllButton;
        [SerializeField] private TMP_Text _text;

        [Inject] private NotificationsLocalization _localization;

        public event Action<NotificationsGroupModel> Clicked;
        
        public override NotificationType Type => NotificationType.NewLikeOnVideo;

        protected override void Awake()
        {
            base.Awake();
            _showAllButton.onClick.AddListener(()=>
            {
                Clicked?.Invoke(Model);
                UpdateText();
            });
        }

        public override void Initialize(NotificationItemModel model)
        {
            base.Initialize(model);
            UpdateText();
        }

        private void UpdateText()
        {
            _text.text = Model.Expanded ? _localization.HideAll : _localization.ShowAll;
        }
    }
}