using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using TMPro;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Color = UnityEngine.Color;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.BlockedAccounts
{
    [RequireComponent(typeof(ZenProjectContextInjecter))]
    internal sealed class BlockedAccountView: MonoBehaviour
    {
        [SerializeField] private RawImage _accountIcon;
        [SerializeField] private TextMeshProUGUI _accountNameText;
        [SerializeField] private Button _button;
        [SerializeField] private ButtonSettings[] _buttonStatesSettings;
        
        [Inject] private IStorageBridge _storageBridge;
        [Inject] private IBlockedAccountsManager _blockUserService;
        [Inject] private SnackBarHelper _snackBarHelper;

        private Profile _blockedProfile;
        private Func<Task> _onButtonClicked;

        private UserState _userState;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private UserState State
        {
            get => _userState;
            set
            {
                _userState = value;
                UpdateButtonState(value);
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Display(Profile profile)
        {
            _blockedProfile = profile;
            State = UserState.Blocked;
            
            LoadThumbnail(profile);
            _accountNameText.text = profile.NickName;
        }
        
        public void Destroy()
        {
            if (_accountIcon.texture != null)
            {
                Destroy(_accountIcon.texture);
                _accountIcon.texture = null;
            }
            Destroy(gameObject);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void LoadThumbnail(Profile profile)
        {
            if (profile.MainCharacter == null) return;
            
            var thumbnailResp = await _storageBridge.GetThumbnailAsync(profile.MainCharacter, Resolution._128x128);
            if (thumbnailResp.IsSuccess)
            {
                _accountIcon.texture = thumbnailResp.Object as Texture2D;
            }
            else
            {
                Debug.LogError($"Failed loading of blocked account thumbnail. Reason: {thumbnailResp.ErrorMessage}");
            }
        }
        
        private async Task BlockUser()
        {
            var result = await _blockUserService.BlockAccount(_blockedProfile);
            if (result.IsSuccess)
            {
                State = UserState.Blocked;
                ShowNotification("Blocked");
            }
            else
            {
                ShowNotification($"Failed to block: {result.ErrorMessage}");
            }
        }
        
        private async Task UnblockUser()
        {
            var result = await _blockUserService.UnblockAccount(_blockedProfile);
            if (result.IsSuccess)
            {
                State = UserState.Unblocked;
                ShowNotification("Unblocked");
            }
            else
            {
                ShowNotification($"Failed to unblock: {result.ErrorMessage}");
            }
        }

        private void ShowNotification(string message)
        {
            _snackBarHelper.ShowInformationSnackBar(message, 2);
        }

        private async void OnButtonClicked()
        {
            await _onButtonClicked.Invoke();
        }

        private void UpdateButtonState(UserState state)
        {
            var settings = _buttonStatesSettings.First(x => x.UserState == state);
            UpdateButtonText(settings.ButtonText, settings.TextColor);
            UpdateButtonBackground(settings.BackgroundImage);
            UpdateCallbackOnClick(state);
        }
        
        private void UpdateButtonText(string text, Color color)
        {
            var textComponent = _button.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.color = color;
        }

        private void UpdateButtonBackground(Sprite sprite)
        {
            _button.GetComponent<Image>().sprite = sprite;
        }
        
        private void UpdateCallbackOnClick(UserState state)
        {
            _onButtonClicked = state == UserState.Blocked ? (Func<Task>) UnblockUser : BlockUser;
        }

        private enum UserState
        {
            Blocked,
            Unblocked
        }

        [Serializable]
        private struct ButtonSettings
        {
            public UserState UserState;
            public string ButtonText;
            public Sprite BackgroundImage;
            public Color TextColor;
        }
    }
}