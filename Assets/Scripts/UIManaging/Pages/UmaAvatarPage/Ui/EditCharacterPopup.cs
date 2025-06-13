using System;
using AdvancedInputFieldPlugin;
using Extensions;
using Modules.CharacterManagement;
using TMPro;
using UIManaging.Common.InputFields;
using UIManaging.Pages.Common.Helpers;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Color = UnityEngine.Color;
using LocalizedString = I2.Loc.LocalizedString;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.UmaAvatarPage.Ui
{
    internal sealed class EditCharacterPopup : BasePopup<EditCharacterPopupConfiguration>
    {
        private const int TOUCHES_LIMIT = 1;
        private const int CHARACTER_LIMIT = 25;
        
        [SerializeField] private AdvancedInputField _characterNameInputField;
        [SerializeField] private Button _setAsMainButton;
        [SerializeField] private Button _usingButton;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _overlayButton;
        [SerializeField] private RawImage _characterRawImage;
        [Header("Localization")]
        [SerializeField] private LocalizedString _mainCharacterSnackbarDesc;

        [Inject] private CharacterManager _characterManager;
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;

        private Action<CharacterInfo> _onEdit;
        private Action<CharacterInfo> _onDelete;
        private Action<CharacterInfo> _onSelect;
        private CharacterInfo _character;
        private IInputFieldAdapter _inputFieldAdapter;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _setAsMainButton.onClick.AddListener(OnSetAsMainButtonClicked);
            _usingButton.onClick.AddListener(OnUsingButtonClicked);
            _editButton.onClick.AddListener(OnEditButtonClicked);
            _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            _overlayButton.onClick.AddListener(OnOverlayButtonClicked);
            
        }

        private void OnDestroy()
        {
            _setAsMainButton.onClick.RemoveListener(OnSetAsMainButtonClicked);
            _usingButton.onClick.RemoveListener(OnUsingButtonClicked);
            _editButton.onClick.RemoveListener(OnEditButtonClicked);
            _deleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
            _overlayButton.onClick.RemoveListener(OnOverlayButtonClicked);
        }
        
        public override void Show()
        {
            base.Show();
            _inputFieldAdapter.OnSubmit += OnNewNameSubmitted;
            _inputFieldAdapter.CharacterLimit = CHARACTER_LIMIT;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Hide()
        {
            base.Hide();
            _inputFieldAdapter.OnSubmit -= OnNewNameSubmitted;  
            _inputFieldAdapter.Dispose();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(EditCharacterPopupConfiguration configuration)
        {
            _inputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_characterNameInputField);
            
            _character = configuration.Character;
            _inputFieldAdapter.Text = configuration.Character.Name;

            _onSelect = configuration.OnSelect;
            _onEdit = configuration.OnEdit;
            _onDelete = configuration.OnDelete;

            _characterManager.CharacterSelected -= OnCharacterSelected;
            _characterManager.CharacterSelected += OnCharacterSelected;

            DownloadCharacterThumbnail();
            RefreshSetAsMainButton();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnSetAsMainButtonClicked()
        {
            if (Input.touchCount > TOUCHES_LIMIT) return;
            _onSelect?.Invoke(_character);
        }

        private void OnUsingButtonClicked()
        {
            ShowInformationSnackBar(_mainCharacterSnackbarDesc);
        }

        private void OnDeleteButtonClicked()
        {
            if (Input.touchCount > TOUCHES_LIMIT) return;
            _onDelete?.Invoke(_character);
        }

        private void OnEditButtonClicked()
        {
            if (Input.touchCount > TOUCHES_LIMIT) return;
            _onEdit?.Invoke(_character);
        }

        private void OnOverlayButtonClicked()
        {
            Hide();
        }

        private void OnNewNameSubmitted(string newName)
        {
            var isNewNameValid = !string.IsNullOrWhiteSpace(newName);

            if (isNewNameValid)
            {
                _characterManager.SetNameForCharacter(_character, newName);
            }
            else
            {
                _inputFieldAdapter.Text = _character.Name;
            }
        }

        private void DownloadCharacterThumbnail()
        {
            SetImageTransparent();
            _characterThumbnailsDownloader.GetCharacterThumbnail(_character, Resolution._512x512,
                OnThumbnailDownloaded, OnFailedToDownloadThumbnail);
        }

        private void OnThumbnailDownloaded(Texture2D texture2D)
        {
            _characterRawImage.color = Color.white;
            _characterRawImage.texture = texture2D;
        }

        private void OnFailedToDownloadThumbnail(string reason)
        {
            SetImageTransparent();
        }

        private void SetImageTransparent()
        {
            _characterRawImage.color = Color.white.SetAlpha(0f);
        }

        private void RefreshSetAsMainButton()
        {
            var isSelected = _characterManager.SelectedCharacter != null && _characterManager.SelectedCharacter.Id == _character.Id;
            _setAsMainButton.gameObject.SetActive(!isSelected);
            _usingButton.gameObject.SetActive(isSelected);
        }

        private void OnCharacterSelected(CharacterInfo character)
        {
            RefreshSetAsMainButton();
        }

        private void ShowInformationSnackBar(string message)
        {
            _snackBarHelper.ShowInformationSnackBar(message, 2);
        }
    }
}