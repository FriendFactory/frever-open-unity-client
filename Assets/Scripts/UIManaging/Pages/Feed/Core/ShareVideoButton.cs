using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Modules.Chat;
using UIManaging.Core;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Feed.Core
{
    internal sealed class ShareVideoButton : ButtonBase
    {
        [SerializeField] private FeedVideoView _feedVideoView;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private IChatService _chatService;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private FeedLocalization _localization;
        [Inject] private ShareToPopupLocalization _shareToPopupLocalization;
        
        protected override void OnClick()
        {
            var configuration = new ShareToPopupConfiguration(ShareVideoToChats, _localUser.GroupId == _feedVideoView.Video.GroupId, _shareToPopupLocalization.SendButton, _feedVideoView.VideoId, blockConfirmButtonIfNoSelection:true);
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(PopupType.ShareTo, true);
        }

        private async void ShareVideoToChats(ShareDestinationData data)
        {
            _popupManagerHelper.ShowWaitVideoPublishOverlay();
            var sharingResult = await _chatService.PostMessage(new SendMessageToGroupsModel
            {
                VideoId = _feedVideoView.VideoId, 
                ChatIds = data.Chats.Select(x=>x.Id).ToArray(), 
                GroupIds = data.Users.Select(x=>x.MainGroupId).ToArray()
            });
            if (sharingResult != null && sharingResult.IsSuccess)
            {
                _snackBarHelper.ShowSuccessDarkSnackBar(_localization.VideoShareSuccessSnackbarMessage);
            }
            else
            {
                _snackBarHelper.ShowFailSnackBar(_localization.VideoShareFailedSnackbarMessage);
            }

            _popupManagerHelper.HideWaitVideoPublishOverlay();
            _popupManager.ClosePopupByType(PopupType.ShareTo);
        }
    }
}