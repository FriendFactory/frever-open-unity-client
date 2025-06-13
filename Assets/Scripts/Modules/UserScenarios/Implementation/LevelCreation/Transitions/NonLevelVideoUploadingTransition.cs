using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.NonLevelVideoUploading;
using Navigation.Args;
using UIManaging.Pages.PublishPage;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class NonLevelVideoUploadingTransition: NonLevelVideoUploadingTransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        public NonLevelVideoUploadingTransition(PublishVideoHelper publishVideoHelper, AmplitudeManager amplitudeManager, ExitToChatPageStateHelper stateHelper) : base(publishVideoHelper, amplitudeManager, stateHelper)
        {
        }

        private VideoUploadingSettings VideoUploadingSettings => Context.PublishContext.VideoPublishSettings;
        private PublishInfo PublishInfo => VideoUploadingSettings.PublishInfo;

        protected override PublishingType PublishingType => VideoUploadingSettings.PublishingType;
        protected override MessagePublishInfo MessagePublishInfo => VideoUploadingSettings.MessagePublishInfo;
        protected override ChatInfo OpenChatOnComplete => Context.OpenedFromChat;
        protected override NonLeveVideoData NonLeveVideoData => Context.PublishContext.NonLevelVideoData;
        protected override VideoAccess Access => PublishInfo.Access;
        protected override string DescriptionText => PublishInfo.DescriptionText;
        protected override long[] SelectedUsers => PublishInfo.SelectedUsers.ToArray();
        protected override ExternalLinkType ExternalLinkType => PublishInfo.ExternalLinkType;
        protected override string ExternalLink => PublishInfo.ExternalLink;
    }
}