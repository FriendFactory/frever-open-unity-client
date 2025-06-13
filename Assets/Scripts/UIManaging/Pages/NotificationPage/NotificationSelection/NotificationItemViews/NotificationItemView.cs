using Abstract;
using Modules.Notifications.NotificationItemModels;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public abstract class NotificationItemView<T> : BaseContextDataView<T> where T : NotificationItemModel
    {
        [SerializeField] private TextMeshProUGUI _timeStampText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Inject] protected NotificationsLocalization _localization;
        
        protected abstract string Description { get; }

        protected override void OnInitialized()
        {
            SetupDescriptionText();
            SetupElapsedTimeText();
        }

        private void SetupDescriptionText()
        {
            _descriptionText.text = Description;
        }
        
        private void SetupElapsedTimeText()
        {
            _timeStampText.text = ContextData.TimeStampText;
        }
    }
}
