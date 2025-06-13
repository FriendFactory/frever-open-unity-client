using System.Collections.Generic;
using System.Threading.Tasks;
using AppsFlyerSDK;
using Bridge;
using Bridge.Models;
using Bridge.VideoServer;
using Common.Publishers;
using Modules.Amplitude;
using Models;
using Modules.AppsFlyerManaging;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using UIManaging.Pages.PublishPage;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    internal abstract class ExitScenarioFromPublishBase : TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private const float RESERVED_PROGRESS_BAR_FOR_VIDEO_UPLOADING = 0.5f;
        
        private readonly ILevelManager _levelManager;
        private readonly PublishVideoHelper _publishHelper;
        private readonly IPublishVideoPopupManager _popupManager;
        private readonly AmplitudeManager _amplitudeManager;
        
        protected readonly IBridge Bridge;

        private PublishContext PublishContext => Context.PublishContext;
        private UploadContext UploadContext => Context.UploadContext;
        private PublishInfo PublishInfo => PublishContext.VideoPublishSettings.PublishInfo;

        protected ExitScenarioFromPublishBase(ILevelManager levelManager, PublishVideoHelper publishHelper, IPublishVideoPopupManager popupManager, AmplitudeManager amplitudeManager, IBridge bridge)
        {
            _levelManager = levelManager;
            _publishHelper = publishHelper;
            _popupManager = popupManager;
            _amplitudeManager = amplitudeManager;
            Bridge = bridge;
        }

        public override ScenarioState To => DestinationState;

        protected ScenarioState DestinationState;

        protected override async Task UpdateContext()
        {
            if (await SetNextState() && UploadContext.UploadOnExit)
            {
                StartVideoDeploying();
            }
        }

        protected override async Task OnRunning()
        {
            _levelManager.CancelLoading();
            _levelManager.ClearTempFiles();

            if (UploadContext.UploadOnExit)
            {
                _popupManager.ShowVideoUploadingPopup(RESERVED_PROGRESS_BAR_FOR_VIDEO_UPLOADING);
            }
            else if (UploadContext.Video != null)
            {
                _publishHelper.ShowPublishSuccessPopup(UploadContext.Video);
            }

            if (Context.SocialActionId != null)
            {
                await Bridge.MarkActionAsComplete(Context.RecommendationId, Context.SocialActionId.Value);
            }
            
            await base.OnRunning();
        }
        
        private void StartVideoDeploying()
        {
            var deployData = new DeployLevelVideoReq
            {
                LocalPath = PublishContext.PortraitVideoFilePath,
                LevelId = Context.LevelData.Id,
                DurationSec = (int) _levelManager.LevelDurationSeconds,
                Access = PublishInfo.Access,
                VideoDescription = PublishInfo.DescriptionText,
                GenerateTemplate = PublishInfo.GenerateTemplate,
                GenerateTemplateWithName = PublishInfo.GenerateTemplateWithName,
                PublishTypeId = Context.PublishContext.PublishingType == PublishingType.VideoMessage
                    ? ServerConstants.VideoPublishingType.VIDEO_MESSAGE 
                    : ServerConstants.VideoPublishingType.STANDARD,
                TaggedFriendIds = PublishInfo.SelectedUsers?.ToArray(),
                Links = new Dictionary<string, string>(),
                AllowRemix = PublishInfo.AllowRemix,
                AllowComment = PublishInfo.AllowComment,
            };

            if (PublishInfo.ExternalLinkType != ExternalLinkType.Invalid)
            {
                var linkTypeStr = PublishInfo.ExternalLinkType.ToString();
                deployData.Links[linkTypeStr] = PublishInfo.ExternalLink;
            }

            var isTaskVideo = Context.Task != null;
            _publishHelper.VideoUploaded += OnVideoUploaded;
            _popupManager.HideVideoRenderingCountdown();
            _publishHelper.UploadLevelVideo(deployData, Context.LevelData, isTaskVideo, OnVideoPublished);
            _levelManager.UseSameFaceFx = false;

            void OnVideoUploaded()
            {
                _popupManager.HideVideoRenderingCountdown();
                _publishHelper.VideoUploaded -= OnVideoUploaded;
            }
        }

        protected abstract Task<bool> SetNextState();

        private async void OnVideoPublished(long videoId)
        {
            AppsFlyer.sendEvent(AppsFlyerConstants.VIDEO_SUCCESSFULLY_CREATED, null);
            SendVideoUploadedAmplitudeEvent(videoId);
            
            if (PublishInfo.SaveToDevice)
            {
                SendVideoSavedToDeviceAmplitudeEvent(videoId);
            }

            await _publishHelper.ShareVideoToChats(videoId, Context.PublishContext.VideoPublishSettings.MessagePublishInfo);
        }
        

        private void SendVideoUploadedAmplitudeEvent(long videoId)
        {
            var videoCreatedMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.VIDEO_ID] = videoId
            };
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.VIDEO_SUCCESSFULLY_CREATED, videoCreatedMetaData);
        }
        
        private void SendVideoSavedToDeviceAmplitudeEvent(long videoId)
        {
            var videoSavedToDeviceMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.VIDEO_ID] = videoId
            };
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.VIDEO_SAVED_TO_DEVICE, videoSavedToDeviceMetaData);
        }
    }
}