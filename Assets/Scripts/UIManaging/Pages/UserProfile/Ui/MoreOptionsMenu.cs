using System;
using System.Collections.Generic;
using Bridge.Services.UserProfile;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui
{
    internal sealed class MoreOptionsMenu : MonoBehaviour
    {
        [Inject] private PopupManagerHelper _popupHelper;
        [Inject] private IBlockedAccountsManager _blockUserService;
        [Inject] private PageManager _pageManager;
        [Inject] private VideoManager _videoManager;//need due to dependencies on entering feed page, must be removed when we refactor feed args
        [Inject] private ProfileLocalization _profileLocalization;
        
        private Profile _profileToBlock;
        
        public void Display(Profile profileToBlock)
        {
            _profileToBlock = profileToBlock;

            ShowBlockUserPopup();
        }

        private void ShowBlockUserPopup()
        {
            _popupHelper.ShowConfirmBlockingPopup(_profileToBlock, OnBlockConfirmed);
        }

        private async void OnBlockConfirmed()
        {
            _pageManager.MoveNext(PageId.Feed, new GeneralFeedArgs(_videoManager));
            
            var response = await _blockUserService.BlockAccount(_profileToBlock);
            if (response.IsSuccess)
            {
                NotifyUserAboutSuccessBlocking();
            }
            else
            {
                Debug.LogError($"Failed blocking account. Reason: {response.ErrorMessage}");
            }
        }

        private void NotifyUserAboutSuccessBlocking()
        {
            _popupHelper.ShowSuccessNotificationPopup(_profileLocalization.UserBlockedSnackbarMessage);
        }
    }
}
