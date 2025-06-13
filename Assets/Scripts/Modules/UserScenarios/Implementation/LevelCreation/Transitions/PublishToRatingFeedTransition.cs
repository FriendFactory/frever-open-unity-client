using System.Collections.Generic;
using System.Threading.Tasks;
using AppsFlyerSDK;
using Bridge.Models;
using Bridge.Models.VideoServer;
using Bridge.VideoServer;
using Common.Publishers;
using JetBrains.Annotations;
using Models;
using Modules.Amplitude;
using Modules.AppsFlyerManaging;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.PublishPage;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal class PublishToRatingFeedTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private const float RESERVED_PROGRESS_BAR_FOR_VIDEO_UPLOADING = 0.5f;

        private readonly ILevelManager _levelManager;
        private readonly PublishVideoHelper _publishHelper;
        private readonly IPublishVideoPopupManager _popupManager;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly PageManager _pageManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override ScenarioState To => ScenarioState.RatingFeed;

        private PublishContext PublishContext => Context.PublishContext;
        private UploadContext UploadContext => Context.UploadContext;
        private PublishInfo PublishInfo => PublishContext.VideoPublishSettings.PublishInfo;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public PublishToRatingFeedTransition(ILevelManager levelManager, PublishVideoHelper publishHelper,
            IPublishVideoPopupManager popupManager, AmplitudeManager amplitudeManager, PageManager pageManager)
        {
            _levelManager = levelManager;
            _publishHelper = publishHelper;
            _popupManager = popupManager;
            _amplitudeManager = amplitudeManager;
            _pageManager = pageManager;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override Task UpdateContext()
        {
            Context.UploadContext.UploadOnExit = false;

            StartVideoDeploying();

            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            _popupManager.ShowVideoUploadingPopup(RESERVED_PROGRESS_BAR_FOR_VIDEO_UPLOADING);
            await base.OnRunning();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

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

            _popupManager.HideVideoRenderingCountdown();
            _publishHelper.VideoPublished += OnVideoPublished;
            _publishHelper.UploadLevelVideo(deployData, Context.LevelData, isTaskVideo, OnVideoUploaded, false);
            _levelManager.UseSameFaceFx = false;
        }

        private void OnVideoUploaded(long videoId)
        {
            AppsFlyer.sendEvent(AppsFlyerConstants.VIDEO_SUCCESSFULLY_CREATED, null);
            SendVideoUploadedAmplitudeEvent(videoId);

            if (PublishInfo.SaveToDevice)
            {
                SendVideoSavedToDeviceAmplitudeEvent(videoId);
            }
        }

        private void OnVideoPublished(Video video)
        {
            _publishHelper.VideoPublished -= OnVideoPublished;

            if (_pageManager.IsCurrentPage(PageId.RatingFeed))
            {
                UploadContext.Video = video;
            }
            else
            {
                _publishHelper.ShowPublishSuccessPopup(video);
            }
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