using Bridge.Models.ClientServer.Chat;
using Common.Publishers;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.NonLevelVideoUploading
{
    [UsedImplicitly]
    internal sealed class PublishNonLevelVideoState : PublishNonLevelVideoStateBase<NonLevelVideoPublishContext>,
                                                      IResolvable
    {
        public PublishNonLevelVideoState(PageManager pageManager) : base(pageManager)
        {
        }

        protected override NonLeveVideoData NonLeveVideoData => Context.NonLeveVideoData;

        protected override PublishingType PublishingType => Context.PublishingType;
        
        protected override ChatInfo OpenChatOnComplete => Context.OpenChatOnComplete;

        protected override void SaveVideoUploadingSettingsToContext(VideoUploadingSettings settings)
        {
            Context.Access = settings.PublishInfo.Access;
            Context.SelectedUsers = settings.PublishInfo.SelectedUsers;
            Context.PublishingType = settings.PublishingType;
            Context.ExternalLinkType = settings.PublishInfo.ExternalLinkType;
            Context.ExternalLink = settings.PublishInfo.ExternalLink;
            Context.DescriptionText = settings.PublishInfo.DescriptionText;
            Context.MessagePublishInfo = settings.MessagePublishInfo;
            
            var nonLeveVideoData = Context.NonLeveVideoData;
            nonLeveVideoData.AllowComment = settings.PublishInfo.AllowComment;
            
            Context.NonLeveVideoData = nonLeveVideoData;
        }

        protected override void SaveNonLevelVideoSettingsToContext(NonLeveVideoData nonLeveVideoData)
        {
            Context.NonLeveVideoData = nonLeveVideoData;
        }
    }
}