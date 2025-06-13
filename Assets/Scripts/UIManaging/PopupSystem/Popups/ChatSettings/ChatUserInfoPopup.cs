using Bridge;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups.ChatSettings
{
    public class ChatUserInfoPopup : BasePopup<ChatUserInfoPopupConfiguration>
    {
        [SerializeField] private UserPortraitView _userPortrait;
        [SerializeField] private TextMeshProUGUI _userName;
        [SerializeField] private Button _profileBtn;
        [SerializeField] private Button _blockBtn;
        [SerializeField] private Button _backBtn;
        [SerializeField] private Button _outsideBtn;
        
        [Inject] private IBlockedAccountsManager _blockUserService;
        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private ChatLocalization _localization;
        
        private void OnEnable()
        {
            _profileBtn.onClick.AddListener(OnProfileButtonClicked);
            _blockBtn.onClick.AddListener(OnBlockButtonClicked);
            _backBtn.onClick.AddListener(Hide);
            _outsideBtn.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            _profileBtn.onClick.RemoveListener(OnProfileButtonClicked);
            _blockBtn.onClick.RemoveListener(OnBlockButtonClicked);
            _backBtn.onClick.RemoveListener(Hide);
            _outsideBtn.onClick.RemoveListener(Hide);
        }

        protected override void OnConfigure(ChatUserInfoPopupConfiguration configuration)
        {
            UpdateUserInfo();
        }

        protected override void OnHidden()
        {
            _userPortrait.CleanUp();
            
            base.OnHidden();
        }

        private void OnProfileButtonClicked()
        {
            _pageManager.MoveNext(new UserProfileArgs(Configs.UserInfo.Id, Configs.UserInfo.Nickname));
            Configs.HideAction?.Invoke(false);
            Hide();
        }

        private void OnBlockButtonClicked()
        {
            var config = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _localization.BlockUserPopupTitle,
                Description =_localization.BlockUserPopupDescription,
                YesButtonText = _localization.BlockUserPopupConfirmButton,
                NoButtonText = _localization.BlockUserPopupCancelButton,
                YesButtonSetTextColorRed = true,
                OnYes = OnBlock,
            };

            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private async void OnBlock() 
        {
            var profileResult = await _bridge.GetProfile(Configs.UserInfo.Id);

            if (profileResult.IsError)
            {
                Debug.LogError($"Failed to fetch user data for user id {Configs.UserInfo.Id}, reason: {profileResult.ErrorMessage}");
                return;
            }

            if (profileResult.IsSuccess)
            {
                var blockResult = await _blockUserService.BlockAccount(profileResult.Profile);

                if (blockResult.IsError)
                {
                    Debug.LogError($"Failed to block user id {Configs.UserInfo.Id}, reason: {blockResult.ErrorMessage}");
                    return;
                }

                if (blockResult.IsSuccess)
                {
                    Configs.HideAction?.Invoke(true);
                    Hide();
                }
            }
        }

        private void UpdateUserInfo()
        {
            _userName.text = Configs.UserInfo.Nickname;
            _blockBtn.SetActive(Configs.UserInfo.Id != _bridge.Profile.GroupId);
            
            if (!Configs.UserInfo.MainCharacterId.HasValue)
            {
                Debug.LogError($"No main character found for user with id {Configs.UserInfo.Id}");
                return;
            }
            
            _userPortrait.Initialize(new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = Configs.UserInfo.Id,
                UserMainCharacterId = Configs.UserInfo.MainCharacterId.Value,
                MainCharacterThumbnail = Configs.UserInfo.MainCharacterFiles
            });    
        }
    }
}