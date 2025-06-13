using System;
using System.Threading.Tasks;
using Common.Publishers;
using Extensions;
using Modules.ContentModeration;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.PublishPage.Buttons;
using UIManaging.Pages.PublishPage.VideoDetails;
using UIManaging.Pages.PublishPage.VideoDetails.Attributes;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    internal abstract class PublishPageBase<TPageArgs, TVideoMessagePanel, TPostPanel> : GenericPage<TPageArgs> 
        where TPageArgs : PageArgs
        where TVideoMessagePanel: VideoMessageSettingsPanelBase
        where TPostPanel: PostVideoSettingsPanelBase
    {
        [SerializeField] protected TPostPanel PostSettingsPanel;
        [SerializeField] protected TVideoMessagePanel VideoMessageSettingsPanel;
        [SerializeField] protected PublishVideoButton PublishButton;
        [SerializeField] protected CanvasGroup MessageCanvasGroup;
        [SerializeField] protected Button MessageLockedButton;
        [SerializeField] protected PostVideoDescriptionPanel PostVideoDescriptionPanel;
        [SerializeField] protected VideoPostAttributesPanel _videoPostAttributesPanel;
        [SerializeField] private Button _settingsButton;
        
        [Inject] protected VideoPostAttributesModel VideoPostAttributesModel;
        [Inject] private TextContentValidator _textValidator;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private PopupManager _popupManager;
        [Inject] private UIManaging.Localization.PublishPageLocalization _localization;
        
        protected abstract PublishingType CurrentPublishingType { get; }

        protected virtual void OnEnable()
        {
            MessageLockedButton.onClick.AddListener(OnMessageLockedClick);
        }
        
        protected virtual void OnDisable()
        {
            MessageLockedButton.onClick.RemoveListener(OnMessageLockedClick);
        }

        private void OnMessageLockedClick()
        {
            var config = new DirectMessagesLockedPopupConfiguration();
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }
        
        protected override void OnDisplayStart(TPageArgs args)
        {
            base.OnDisplayStart(args);

            var isLocked = _localUser.UserProfile.ChatAvailableAfterTime > DateTime.UtcNow;

            MessageCanvasGroup.alpha = isLocked ? 0.5f : 1;
            MessageLockedButton.SetActive(isLocked);
            
            PostVideoDescriptionPanel.Initialize();
            _videoPostAttributesPanel.Initialize(VideoPostAttributesModel);
            
            _settingsButton.onClick.AddListener(ShowSettingsPanel);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            _settingsButton.onClick.RemoveListener(ShowSettingsPanel);
            
            PostVideoDescriptionPanel.CleanUp();
            _videoPostAttributesPanel.CleanUp();
        }

        protected abstract void ShowSettingsPanel();
        protected abstract void RefreshPageSettings();

        protected async Task<bool> ValidateDestination()
        {
            if (CurrentPublishingType != PublishingType.VideoMessage || HasDestinationsForSharingVideoMessage())
                return true;
            
            await ForceUserToPickSendDestinations();
            return HasDestinationsForSharingVideoMessage();
        }
        
        protected Task<bool> ValidateText()
        {
            var textToValidate = CurrentPublishingType == PublishingType.Post
                ? PostVideoDescriptionPanel.ParsedDescriptionText
                : VideoMessageSettingsPanel.Message;
            if (string.IsNullOrEmpty(textToValidate)) return Task.FromResult(true);

            var failValidationMessage = CurrentPublishingType == PublishingType.Post
                ? _localization.DescriptionModerationFailedSnackbarMessage
                : _localization.MessageDescriptionModerationFailedSnackbarMessage;
            return _textValidator.ValidateTextContent(textToValidate, failValidationMessage);
        }
        
        protected void OnPublishingForbidden()
        {
            PublishButton.Interactable = true;
        }

        private async Task ForceUserToPickSendDestinations()
        {
            var destinationPicked = false;
            var selectionCancelled = false;

            VideoMessageSettingsPanel.OpenSendDestinationSelection();
            VideoMessageSettingsPanel.DestinationSelected += OnDestinationPicked;
            VideoMessageSettingsPanel.DestinationSelectionCancelled += OnSelectionCancelled;
            
            while (!destinationPicked && !selectionCancelled)
            {
                await Task.Delay(33);
            }
            
            void OnDestinationPicked(ShareDestinationData data)
            {
                destinationPicked = true;
                Unsubscribe();
            }

            void OnSelectionCancelled()
            {
                selectionCancelled = true;
                Unsubscribe();
            }

            void Unsubscribe()
            {
                VideoMessageSettingsPanel.DestinationSelected -= OnDestinationPicked;
                VideoMessageSettingsPanel.DestinationSelectionCancelled -= OnSelectionCancelled;
            }
        }
        
        private bool HasDestinationsForSharingVideoMessage()
        {
            return VideoMessageSettingsPanel.ShareDestinationData is { HasAny: true };
        }
    }
}