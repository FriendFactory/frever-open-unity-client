using Bridge.Models.ClientServer.Chat;
using Common.Publishers;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.NonLevelVideoUploading;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal sealed class PublishNonLevelVideoState : PublishNonLevelVideoStateBase<ILevelCreationScenarioContext>, IResolvable
    {
        public PublishNonLevelVideoState(PageManager pageManager) : base(pageManager)
        {
        }
        
        private PublishContext PublishContext => Context.PublishContext;
        
        
        protected override NonLeveVideoData NonLeveVideoData => PublishContext.NonLevelVideoData;
        protected override PublishingType PublishingType => PublishContext.VideoPublishSettings.PublishingType;

        protected override ChatInfo OpenChatOnComplete => Context.OpenedFromChat;

        protected override void SaveVideoUploadingSettingsToContext(VideoUploadingSettings settings)
        {
            Context.PublishContext.VideoPublishSettings = settings;

            var nonLeveVideoData = Context.PublishContext.NonLevelVideoData;
            nonLeveVideoData.AllowComment = Context.PublishContext.VideoPublishSettings.PublishInfo.AllowComment;
            
            Context.PublishContext.NonLevelVideoData = nonLeveVideoData;
        }

        protected override void SaveNonLevelVideoSettingsToContext(NonLeveVideoData nonLeveVideoData)
        {
            Context.PublishContext.NonLevelVideoData = nonLeveVideoData;
        }
    }
}