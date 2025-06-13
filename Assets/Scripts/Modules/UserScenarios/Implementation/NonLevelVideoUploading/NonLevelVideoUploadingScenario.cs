using System.Threading.Tasks;
using Common.Publishers;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.NonLevelVideoUploading
{
    [UsedImplicitly]
    internal sealed class NonLevelVideoUploadingScenario : ScenarioBase<NonLevelVideoUploadArgs,NonLevelVideoPublishContext>, IUploadGalleryVideoScenario
    {
        public NonLevelVideoUploadingScenario(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs);
        }

        protected override Task<NonLevelVideoPublishContext> SetupContext()
        {
            return Task.FromResult(new NonLevelVideoPublishContext());
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var galleryUploadState = ResolveState<PublishNonLevelVideoState>();
            var exitBack = ResolveState<PreviousPageExitState>();
            var exitToChat = ResolveState<ChatPageExitState>();
            var exitToInbox = ResolveState<InboxPageExitState>();
            var exitToCrewPage = ResolveState<CrewPageExitState>();
            var exitToProfile = ResolveState<ProfileExitState>();
            galleryUploadState.OnBack = new EmptyTransition(exitBack);
            galleryUploadState.OnVideoPublishingRequested = ResolveTransition<NonLevelVideoUploadingTransition>();

            return Task.FromResult(new IScenarioState[] { galleryUploadState, exitBack, exitToChat, exitToInbox, exitToCrewPage, exitToProfile });
        }

        private sealed class EntryTransition: EntryTransitionBase<NonLevelVideoPublishContext>
        {
            private readonly NonLevelVideoUploadArgs _args;
            public override ScenarioState To => ScenarioState.PublishFromGallery;

            public EntryTransition(NonLevelVideoUploadArgs scenarioArgs) : base(scenarioArgs)
            {
                _args = scenarioArgs;
            }

            protected override Task UpdateContext()
            {
                Context.NonLeveVideoData = _args.NonLeveVideoData;
                Context.OpenChatOnComplete = _args.OpenChatOnComplete;
                if (Context.OpenChatOnComplete != null)
                {
                    Context.PublishingType = PublishingType.VideoMessage;
                }
                return base.UpdateContext();
            }
        }
    }
}