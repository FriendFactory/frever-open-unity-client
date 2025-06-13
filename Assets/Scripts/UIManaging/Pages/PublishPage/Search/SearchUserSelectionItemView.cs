using System;
using Extensions;
using UIManaging.Common.SearchPanel;
using UIManaging.Localization;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.FollowersPage.UI.Search
{
    public class SearchUserSelectionItemView : SearchUserBaseItemView
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _lockedButton;

        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private ProfileLocalization _profileLocalization;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var isLocked = ContextData.ChatAvailableAfterTime > DateTime.UtcNow;

            _canvasGroup.alpha = isLocked ? 0.5f : 1;
            _lockedButton.SetActive(isLocked);
            _lockedButton.onClick.AddListener(OnLockedClick);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _lockedButton.onClick.RemoveListener(OnLockedClick);
        }

        private void OnLockedClick()
        {
            _snackBarHelper.ShowMessagesLockedSnackBar(_profileLocalization.MessagesLockedSnackBar);
        }
    }
}