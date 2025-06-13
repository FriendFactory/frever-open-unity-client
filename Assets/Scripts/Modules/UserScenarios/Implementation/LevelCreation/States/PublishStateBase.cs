using System.Collections.Generic;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.Amplitude;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Navigation.Args;
using UIManaging.Pages.PublishPage;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal abstract class PublishStateBase: StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly IPublishVideoPopupManager _videoPublishingPopupManager;
        private readonly IPublishVideoController _publishVideoController;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly ExitToChatPageStateHelper _exitToChatPageStateHelper;

        public override ITransition[] Transitions => new[] { MoveNextChat, MoveNextPublic, MoveNextLimitedAccess, PreviewRequested }.RemoveNulls();

        public ITransition MoveNextChat;
        public ITransition MoveNextPublic;
        public ITransition MoveNextLimitedAccess;
        public ITransition PreviewRequested;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private PublishContext PublishContext => Context.PublishContext;
        private PublishInfo VideoPublishSettings => PublishContext.VideoPublishSettings.PublishInfo;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected PublishStateBase(IPublishVideoPopupManager videoPublishingPopupManager, IPublishVideoController publishVideoController, AmplitudeManager amplitudeManager)
        {
            _videoPublishingPopupManager = videoPublishingPopupManager;
            _publishVideoController = publishVideoController;
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Run()
        {
            _publishVideoController.OnRefocus += OnRefocus;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void OnPublishRequested(VideoUploadingSettings publishInfo)
        {
            SyncContext(publishInfo);
            StartVideoPublishingProcess(Context.TaskId != null);
        }

        protected async void OnPreviewRequested(Level level, VideoUploadingSettings videoUploadingSettings)
        {
            Context.LevelData = level;
            SyncContext(videoUploadingSettings);
            await PreviewRequested.Run();

            UnsubscribeFromRefocus();
        }

        protected void UnsubscribeFromRefocus()
        {
            _publishVideoController.OnRefocus -= OnRefocus;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnRefocus()
        {
            _videoPublishingPopupManager.HideVideoRenderingCountdown();
            var restartCaptureRequested = false;
            var cancelCaptureRequested = false;
            _videoPublishingPopupManager.ShowRestartRenderingOptionPopup(()=>
            {
                OnRestartCaptureRequested();
                restartCaptureRequested = true;
            }, ()=>
            {
                cancelCaptureRequested = true;
                OnRenderingCancelled();
            }, x =>
            {
                if (restartCaptureRequested || cancelCaptureRequested) return;
                OnRenderingCancelled();
            } );
        }

        private void StartVideoPublishingProcess(bool isTaskVideo)
        {
            _videoPublishingPopupManager.ShowVideoRenderingPopup(_publishVideoController.CurrentVideoRenderingState, isTaskVideo, OnRenderingCancelled);
            _publishVideoController.RenderVideo(VideoPublishSettings.SaveToDevice, VideoPublishSettings.IsLandscapeMode, VideoPublishSettings.IsUltraHDMode, OnRenderingCompleted);
        }

        private void OnRenderingCancelled()
        {
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CANCEL_PUBLISHING_BUTTON_PRESSED,
                                                          new Dictionary<string, object> { ["LevelId"] = Context.LevelData.Id } );
            _videoPublishingPopupManager.HideLoadingOverlay();
            _videoPublishingPopupManager.HideVideoRenderingCountdown();

            _publishVideoController.Cancel();
        }

        private async void OnRenderingCompleted()
        {
            Context.PublishContext.PortraitVideoFilePath = _publishVideoController.PortraitVideoFilePath;

            var messagePublishInfo = Context.PublishContext.VideoPublishSettings.MessagePublishInfo;
            if (Context.OpenedFromChat != null ||
                PublishContext.PublishingType == PublishingType.VideoMessage &&
                _exitToChatPageStateHelper.HasUserSelectedDirectMessageVideoSend(messagePublishInfo))
            {
                await MoveNextChat.Run();
            }
            else if (Context.PublishContext.VideoPublishSettings.PublishInfo.Access == VideoAccess.Public)
            {
                await MoveNextPublic.Run();
            }
            else
            {
                await MoveNextLimitedAccess.Run();
            }

            UnsubscribeFromRefocus();
        }
        
        private void SyncContext(VideoUploadingSettings videoUploadingSettings)
        {
            Context.PublishContext.PublishingType = videoUploadingSettings.PublishingType;
            Context.PublishContext.VideoPublishSettings = videoUploadingSettings;
            Context.OnClearPrivacyData = videoUploadingSettings.PublishInfo.OnClearPublishData;
            Context.PublishContext.VideoPublishSettings.MessagePublishInfo = videoUploadingSettings.MessagePublishInfo;
        }
        
        private void OnRestartCaptureRequested()
        {
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.RESTART_PUBLISHING_BUTTON_PRESSED,
                                                          new Dictionary<string, object>
                                                              { ["LevelId"] = Context.LevelData.Id });
            StartVideoPublishingProcess(Context.TaskId != null);
        }

    }
}