using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Navigation.Args;
using UIManaging.Pages.PublishPage;

namespace Modules.UserScenarios.Implementation.NonLevelVideoUploading
{
    [UsedImplicitly]
    internal sealed class NonLevelVideoUploadingTransition: NonLevelVideoUploadingTransitionBase<NonLevelVideoPublishContext>, IResolvable
    {
        public NonLevelVideoUploadingTransition(PublishVideoHelper publishVideoHelper, AmplitudeManager amplitudeManager, ExitToChatPageStateHelper stateHelper) : base(publishVideoHelper, amplitudeManager, stateHelper)
        {
        }

        protected override PublishingType PublishingType => Context.PublishingType;
        protected override MessagePublishInfo MessagePublishInfo => Context.MessagePublishInfo;
        protected override ChatInfo OpenChatOnComplete => Context.OpenChatOnComplete;
        protected override NonLeveVideoData NonLeveVideoData => Context.NonLeveVideoData;
        protected override VideoAccess Access => Context.Access;
        protected override string DescriptionText => Context.DescriptionText;
        protected override long[] SelectedUsers => Context.SelectedUsers.ToArray();
        protected override ExternalLinkType ExternalLinkType => Context.ExternalLinkType;
        protected override string ExternalLink => Context.ExternalLink;

        protected override void OnVideoUploaded(long videoId)
        {
            base.OnVideoUploaded(videoId);
            Context.UploadedVideoId = videoId;
        }
    }
}