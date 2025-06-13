using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Models;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using Bridge.VideoServer;
using Common.Publishers;
using Modules.Amplitude;
using Models;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Navigation.Args;
using UIManaging.Pages.PublishPage;

namespace Modules.UserScenarios.Implementation.NonLevelVideoUploading
{
    internal abstract class NonLevelVideoUploadingTransitionBase<TContext>: TransitionBase<TContext>
    {
        private readonly PublishVideoHelper _publishVideoHelper;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly ExitToChatPageStateHelper _stateHelper;
        private ScenarioState _destinationState;

        public override ScenarioState To => _destinationState;
        
        protected abstract PublishingType PublishingType { get; }
        protected abstract MessagePublishInfo MessagePublishInfo { get; }
        protected abstract ChatInfo OpenChatOnComplete { get; }
        protected abstract NonLeveVideoData NonLeveVideoData { get; }
        protected abstract VideoAccess Access { get; }
        protected abstract string DescriptionText { get; }
        protected abstract long[] SelectedUsers { get; }
        protected abstract ExternalLinkType ExternalLinkType { get; }
        protected abstract string ExternalLink { get; }

        protected NonLevelVideoUploadingTransitionBase(PublishVideoHelper publishVideoHelper, AmplitudeManager amplitudeManager, ExitToChatPageStateHelper stateHelper)
        {
            _publishVideoHelper = publishVideoHelper;
            _amplitudeManager = amplitudeManager;
            _stateHelper = stateHelper;
        }

        protected override Task UpdateContext()
        {
            if (OpenChatOnComplete!= null || PublishingType == PublishingType.VideoMessage && _stateHelper.HasUserSelectedDirectMessageVideoSend(MessagePublishInfo))
            {
                _destinationState = _stateHelper.GetDestinationForDirectMessageSharing(OpenChatOnComplete, MessagePublishInfo);
            }
            else
            {
                _destinationState = Access == VideoAccess.Private? ScenarioState.ProfileExit : ScenarioState.PreviousPageExit;
            }
            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            var isUploading = true;
            var publishType = PublishingType == PublishingType.VideoMessage 
                ? ServerConstants.VideoPublishingType.VIDEO_MESSAGE 
                : ServerConstants.VideoPublishingType.STANDARD;
            var deployData = new DeployNonLevelVideoReq(NonLeveVideoData.Path, NonLeveVideoData.DurationSec,
                                                        Access, publishType, DescriptionText,
                                                        links: new Dictionary<string, string>(),
                                                        taggedFriendIds: SelectedUsers)
            {
                AllowComment = NonLeveVideoData.AllowComment,
            };
            
            if (ExternalLinkType != ExternalLinkType.Invalid)
            {
                var linkTypeStr = ExternalLinkType.ToString();
                deployData.Links[linkTypeStr] = ExternalLink;
            }
            
            _publishVideoHelper.UploadNonLevelVideo(deployData, OnComplete, OnVideoUploadFailed);
            while (isUploading)
            {
                await Task.Delay(33);
            }
            
            async void OnComplete(long id)
            {
                OnVideoUploaded(id);
                isUploading = false;
                LogUploadedVideoAmplitudeEvent(id);
                await _publishVideoHelper.ShareVideoToChats(id, MessagePublishInfo);
            }

            void OnVideoUploadFailed()
            {
                isUploading = false;
            }
            await base.OnRunning();
        }

        protected virtual void OnVideoUploaded(long videoId)
        {
        }
        
        private void LogUploadedVideoAmplitudeEvent(long videoId)
        {
            var videoCreatedMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.VIDEO_ID] = videoId
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.VIDEO_SUCCESSFULLY_CREATED,
                                                          videoCreatedMetaData);
        }
    }
}