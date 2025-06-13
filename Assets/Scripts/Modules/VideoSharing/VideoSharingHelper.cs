using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.ClientServer.Chat;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using JetBrains.Annotations;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;

namespace Modules.VideoSharing
{
    [UsedImplicitly]
    public sealed class VideoSharingHelper
    {
        private readonly PopupManager _popupManager;
        private readonly PopupManagerHelper _popupManagerHelper;
        private readonly IChatService _chatService;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly LocalUserDataHolder _localUser;
        private readonly FeedLocalization _localization;
        private readonly ShareToPopupLocalization _shareToPopupLocalization;
        private readonly IBridge _bridge;
        private readonly NativeVideoShare _nativeVideoShare;

        public VideoSharingHelper(PopupManager popupManager, PopupManagerHelper popupManagerHelper, IChatService chatService, SnackBarHelper snackBarHelper, LocalUserDataHolder localUser, FeedLocalization localization, ShareToPopupLocalization shareToPopupLocalization, IBridge bridge)
        {
            _popupManager = popupManager;
            _popupManagerHelper = popupManagerHelper;
            _chatService = chatService;
            _snackBarHelper = snackBarHelper;
            _localUser = localUser;
            _localization = localization;
            _shareToPopupLocalization = shareToPopupLocalization;
            _bridge = bridge;

            _nativeVideoShare = new NativeVideoShare();
        }

        public void Share(Video video)
        {
            var configuration = new ShareToPopupConfiguration(ShareVideoToChats, _localUser.GroupId == video.GroupId,
                                                              _shareToPopupLocalization.SendButton, video.Id,
                                                              blockConfirmButtonIfNoSelection: true);
            
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(PopupType.ShareTo, true);

            async void ShareVideoToChats(ShareDestinationData data)
            {
                _popupManagerHelper.ShowWaitVideoPublishOverlay();
                var sharingResult = await _chatService.PostMessage(new SendMessageToGroupsModel
                {
                    VideoId = video.Id,
                    ChatIds = data.Chats.Select(x => x.Id).ToArray(),
                    GroupIds = data.Users.Select(x => x.MainGroupId).ToArray()
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

        public async Task<NativeVideoShareResult> ShareLinkNativeAsync(string personalizedVideoUrl)
        {
            return await _nativeVideoShare.ShareVideoLinkAsync(personalizedVideoUrl);
        }
    }
}