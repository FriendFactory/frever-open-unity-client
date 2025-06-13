using AdvancedInputFieldPlugin;
using Bridge;
using Modules.ContentModeration;
using StansAssets.Foundation.Patterns;
using UIManaging.Localization;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    internal class SoundSettingsView: MusicViewBase<SoundSettingsViewModel>
    {
        private const string NOT_UNIQUE_ERROR_CODE = "UserSoundNameNotUnique";
        
        [SerializeField] private AdvancedInputField _inputField;
        [SerializeField] private Button _saveButton;

        [Inject] private IClientServerBridge _bridge;
        [Inject] private TextContentValidator _textContentValidator;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private MusicGalleryLocalization _localization;
        
        private long SoundId { get; set; }
        
        protected override string Name => _localization.UserSoundSettingsHeader;

        protected override void OnInitialized(SoundSettingsViewModel model)
        {
            SoundId = model.UserSoundFullInfo.Id;
        }

        protected override void OnActivated()
        {
            _saveButton.onClick.AddListener(UpdateSoundName);
            _inputField.OnValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDeactivated()
        {
            _saveButton.onClick.RemoveListener(UpdateSoundName);
        }

        protected override void OnShowContent()
        {
            base.OnShowContent();
            
            _saveButton.interactable = false;
        }

        protected override void OnHideContent()
        {
            base.OnHideContent();

            _inputField.Text = string.Empty;
        }

        protected override void MoveBack() => _stateSelectionController.Fire(MusicNavigationCommand.CloseSoundSettings);
        
        private void OnValueChanged(string value)
        {
            _saveButton.interactable = value.Length > 0;
        }
        
        private async void UpdateSoundName()
        {
            var soundName = _inputField.Text;

            var moderationPassed = await _textContentValidator.ValidateTextContent(soundName);

            if (!moderationPassed) return;

            var result = await _bridge.UpdateUserSoundNameAsync(SoundId, soundName);
            if (result.IsError)
            {
                ShowErrorSnackBar(result.ErrorMessage);
                return;
            }
            
            StaticBus<UserSoundUpdatedEvent>.Post(new UserSoundUpdatedEvent(result.Model));
            MoveBack();
        }

        private void ShowErrorSnackBar(string errorMessage)
        {
            var message = errorMessage.Contains(NOT_UNIQUE_ERROR_CODE)
                ? _localization.UserSoundSettingsNotUniqueErrorMessage 
                : _localization.UserSoundSettingsErrorMessage;
            
            _snackBarHelper.ShowFailSnackBar(message);
        }
    }
}