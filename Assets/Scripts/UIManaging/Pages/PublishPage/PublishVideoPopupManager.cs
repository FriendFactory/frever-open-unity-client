using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    [UsedImplicitly]
    internal sealed class PublishVideoPopupManager: IPublishVideoPopupManager
    {
        [Inject] private LoadingOverlayLocalization _localization;
        
        private static readonly List<string> POSSIBLE_EXIT_SCENES = new List<string>()
        {
            "ProfileScene",
            "FeedScene",
            "CrewScene",
            "RatingFeedScene",
        };

        private readonly PopupManager _popupManager;
        
        private VideoPublishingLoadingPagePopupConfiguration _publishingPopupConfiguration;

        public PublishVideoPopupManager(PopupManager popupManager)
        {
            _popupManager = popupManager;
        }

        public void ShowVideoRenderingPopup(IVideoRenderingState videoRenderingState, bool isTaskVideo, Action onCanceled)
        {
            var header = isTaskVideo
                ? _localization.TaskVideoSubmittingHeader
                : _localization.VideoPublishingHeader;
            _publishingPopupConfiguration = new VideoPublishingLoadingPagePopupConfiguration(header, _localization.SavingProgressMessage, videoRenderingState);
            _publishingPopupConfiguration.SetMaxProgressValue(0.5f);
            _publishingPopupConfiguration.CancelActionRequested = onCanceled;
            _publishingPopupConfiguration.HideAfterSceneSwitch(POSSIBLE_EXIT_SCENES);
            
            videoRenderingState.ProgressUpdated += _publishingPopupConfiguration.UpdateProgress;

            _popupManager.ClosePopupByType(_publishingPopupConfiguration.PopupType);
            _popupManager.SetupPopup(_publishingPopupConfiguration);
            _popupManager.ShowPopup(_publishingPopupConfiguration.PopupType);
        }
        
        public void ShowVideoUploadingPopup(float initialProgressValue)
        {
            var configuration = new VideoUploadingCountdownPopupConfiguration(initialProgressValue);
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);            
        }

        public void HideLoadingOverlay()
        {
            _publishingPopupConfiguration.Hide();
            _publishingPopupConfiguration.CleanUp();
        }

        public void HideVideoRenderingCountdown()
        {
            _popupManager.ClosePopupByType(PopupType.VideoRenderingCountdown);
        }

        public void ShowRestartRenderingOptionPopup(Action onRestartRenderingClicked, Action onCancelClicked, Action<object> onClose)
        {
            var popupConfig = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheet,
                Description = "Capture was canceled",
                Variants = new List<KeyValuePair<string, Action>>
                {
                    new KeyValuePair<string, Action>("Restart capture", onRestartRenderingClicked)
                },
                OnCancel = onCancelClicked,
                OnClose = onClose
            };
            
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(PopupType.ActionSheet);
        }
    }
}