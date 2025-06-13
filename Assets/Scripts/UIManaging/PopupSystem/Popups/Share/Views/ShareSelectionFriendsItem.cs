using System;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups.Share
{
    public sealed class ShareSelectionFriendsItem : ShareSelectionItem<ShareSelectionFriendsItemModel>
    {
        [SerializeField] private UserPortraitView _userPortraitView;

        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private ProfileLocalization _profileLocalization;

        protected override void RefreshPortraitImage()
        {
            if (ContextData.Profile?.MainCharacter == null) return;
            
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = ContextData.Profile.MainGroupId,
                UserMainCharacterId = ContextData.Profile.MainCharacter.Id,
                MainCharacterThumbnail = ContextData.Profile.MainCharacter.Files,
            };

            _userPortraitView.Initialize(userPortraitModel);
        }

        protected override void OnLockedClicked()
        {
            if (ContextData.Profile.ChatAvailableAfterTime > DateTime.UtcNow)
            {
                _snackBarHelper.ShowMessagesLockedSnackBar(_profileLocalization.MessagesLockedSnackBar);
                return;
            }
            
            base.OnLockedClicked();
        }
    }
}