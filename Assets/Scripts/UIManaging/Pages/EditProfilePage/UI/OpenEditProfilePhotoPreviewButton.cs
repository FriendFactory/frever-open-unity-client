using System;
using System.Collections.Generic;
using UIManaging.Pages.UserProfile.Ui.ProfileHelper;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    internal class OpenEditProfilePhotoPreviewButton : OpenProfilePhotoPreviewButtonBase
    {
        protected override BaseUserProfileHelper UserProfileHelper => _localUserProfileHelper;

        [Inject] private LocalUserProfileHelper _localUserProfileHelper;
        [Inject] private PopupManager _popupManager;

        protected override void OnClick()
        {
            OpenConfirmChangePopup(base.OnClick);
        }

        private void OpenConfirmChangePopup(Action onConfirm)
        {
            var configuration = new ActionSheetPopupConfiguration()
            {
                PopupType = PopupType.ActionSheet,
                Variants = new List<KeyValuePair<string, Action>>()
                    { new KeyValuePair<string, Action>("Change profile image", onConfirm) },
                MainVariantIndexes = new[] { 1 },
            };

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(PopupType.ActionSheet);
        }
    }
}