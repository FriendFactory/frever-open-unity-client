using Common.Abstract;
using Navigation.Core;
using UIManaging.Pages.AppSettingsPage.UI.Args;
using UIManaging.Pages.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class UnverifiedAccountBannerPanel: BaseContextPanel<NotificationBadgeModel>
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private Button _verifyButton;

        [Inject] private PageManager _pageManager;

        protected override bool IsReinitializable => true;

        protected override void OnInitialized()
        {
            UpdateBanner(ContextData.NotificationCount);
            
            ContextData.NotificationCountChanged += UpdateBanner;
            
            _verifyButton.onClick.AddListener(MoveToManageAccountPage);
        }

        private void MoveToManageAccountPage()
        {
            _pageManager.MoveNext(new ManageAccountPageArgs());
        }

        protected override void BeforeCleanUp()
        {
            ContextData.NotificationCountChanged -= UpdateBanner;
            
            _verifyButton.onClick.RemoveListener(MoveToManageAccountPage);
        }

        private void UpdateBanner(int notificationCount)
        {
            gameObject.SetActive(notificationCount > 0);
        }
    }
}