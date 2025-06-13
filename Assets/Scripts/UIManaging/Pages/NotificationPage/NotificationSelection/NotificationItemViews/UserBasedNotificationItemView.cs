using System;
using Modules.Notifications.NotificationItemModels;
using TMPro;
using UIManaging.Common;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public abstract class UserBasedNotificationItemView<T> : UserTimestampItemView<T> where T : NotificationItemModel
    {
        [SerializeField] protected TextMeshProUGUI _descriptionText;
        [SerializeField] protected Button _descriptionTextButton;

        [Inject] protected NotificationsLocalization _localization;
        
        public event Action DescriptionSet;

        protected abstract string Description { get; }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _descriptionTextButton.onClick.RemoveListener(OnDescriptionTextClick);
        }

        protected virtual void OnSetupComplete()
        {
        }

        protected virtual void SetupDescriptionText()
        {
            var userName = UserProfile == null ? "Unknown" : UserProfile.NickName;
            _descriptionText.text = string.Format(Description, userName);
            InvokeDescriptionSet();
        }

        protected override void OnUserProfileDownloadSuccess()
        {
            base.OnUserProfileDownloadSuccess();
            _descriptionTextButton.onClick.AddListener(OnDescriptionTextClick);
            SetupDescriptionText();
            OnSetupComplete();
        }

        protected override void OnUserProfileDownloadFailed()
        {
            base.OnUserProfileDownloadFailed();
            SetupDescriptionText();
        }

        protected virtual void OnDescriptionTextClick()
        {
            PrefetchUser();
        }

        protected void InvokeDescriptionSet()
        {
            DescriptionSet?.Invoke();
        }
    }
}