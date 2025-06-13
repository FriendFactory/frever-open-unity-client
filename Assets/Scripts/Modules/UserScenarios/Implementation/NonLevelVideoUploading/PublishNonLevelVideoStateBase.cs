using Bridge.Models.ClientServer.Chat;
using Common.Publishers;
using Extensions;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.NonLevelVideoUploading
{
    internal abstract class PublishNonLevelVideoStateBase<TContext> : StateBase<TContext>
    {
        private readonly PageManager _pageManager;
        
        public ITransition OnBack;
        public ITransition OnVideoPublishingRequested;

        protected PublishNonLevelVideoStateBase(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override ScenarioState Type => ScenarioState.PublishFromGallery;
        public override ITransition[] Transitions => new[] { OnBack, OnVideoPublishingRequested }.RemoveNulls();
        
        protected abstract NonLeveVideoData NonLeveVideoData { get; }
        protected abstract PublishingType PublishingType { get;}
        protected abstract ChatInfo OpenChatOnComplete { get; }
        
        public override void Run()
        {
            var args = new PublishGalleryVideoPageArgs
            {
                VideoData = NonLeveVideoData,
                OnMoveBack = () => OnBack.Run(),
                PublishingType = PublishingType,
                OnMoveForward = x =>
                {
                    SaveVideoUploadingSettingsToContext(x);
                    SaveNonLevelVideoSettingsToContext(NonLeveVideoData);
                    OnVideoPublishingRequested.Run();
                }
            };
            if (OpenChatOnComplete != null)
            {
                args.ShareDestination = new ShareDestination
                {
                    Chats = new[] { OpenChatOnComplete.ToChatShortInfo() }
                };
            }
            _pageManager.MoveNext(args);
        }

        protected abstract void SaveVideoUploadingSettingsToContext(VideoUploadingSettings settings);
        protected abstract void SaveNonLevelVideoSettingsToContext(NonLeveVideoData nonLeveVideoData);
    }
}