using System.Threading.Tasks;
using Bridge.Models;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.VideoServer;
using Common;
using Common.Publishers;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsStoraging.Core;
using Modules.EditorsCommon;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.TempSaves.Manager;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Navigation.Args;
using Zenject;
using PublishNonLevelVideoState = Modules.UserScenarios.Implementation.LevelCreation.States.PublishNonLevelVideoState;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    [UsedImplicitly]
    internal sealed class VideoMessageCreationScenario: LevelCreationScenarioBase<VideoMessageCreationScenarioArgs>, IVideoMessageCreationScenario
    {
        public VideoMessageCreationScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider) : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, Resolve<ILevelForVideoMessageProvider>(), Resolve<TempFileManager>(), Resolve<IMetadataProvider>());
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var videoMessageEditorState = ResolveState<VideoMessageEditorState>();
            var levelPreviewEditorState = ResolveState<PostRecordEditorState>();
            var exitState = ResolveState<PreviousPageExitState>();
            var publishState = ResolveState<PublishState>();
            var publishGalleryVideoState = ResolveState<PublishNonLevelVideoState>();
            var feedExit = ResolveState<FeedExitState>();
            var profileExit = ResolveState<ProfileExitState>();
            var inboxPageExit = ResolveState<InboxPageExitState>();
            var chatPageExit = ResolveState<ChatPageExitState>();
            var crewPageExit = ResolveState<CrewPageExitState>();
            var exitToLevelCreation = ResolveState<ExitToCreationNewLevelScenarioState>();
            
            videoMessageEditorState.MoveBack = new EmptyTransition(exitState);
            videoMessageEditorState.MoveNext = ResolveTransition<VideoMessageEditorToPublishTransition>();
            videoMessageEditorState.ExitToLevelCreation = ResolveTransition<VideoMessageToLevelCreationScenarioTransition>();
            videoMessageEditorState.NonLevelVideoUploading = ResolveTransition<VideoMessageEditorToPublishNonLevelVideoTransition>();
            publishGalleryVideoState.OnBack = new EmptyTransition(videoMessageEditorState);
            publishGalleryVideoState.OnVideoPublishingRequested = ResolveTransition<NonLevelVideoUploadingTransition>();
            publishState.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publishState.MoveNextPublic = ResolveTransition<ExitScenarioFromPublish>();
            publishState.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
            publishState.MoveBack = new EmptyTransition(videoMessageEditorState);
            publishState.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publishState.SaveToDraftsRequested = new EmptyTransition(exitState);
            levelPreviewEditorState.OnPreviewFinished = ResolveTransition<BackToPublishScreenAfterVideoMessagePreviewTransition>();

            return Task.FromResult(new IScenarioState[] { videoMessageEditorState, levelPreviewEditorState, publishState, exitState, feedExit, profileExit, inboxPageExit, chatPageExit, exitToLevelCreation, publishGalleryVideoState, crewPageExit });
        }

        public override void OnExit()
        {
            var backgroundsCache = Resolve<ISetLocationBackgroundInMemoryCacheControl>();
            backgroundsCache.Clear();
            base.OnExit();
        }

        [UsedImplicitly]
        private sealed class EntryTransition: EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly ILevelForVideoMessageProvider _levelForVideoMessageProvider;
            private readonly VideoMessageCreationScenarioArgs _args;
            private readonly TempFileManager _tempFileManager;
            private readonly IMetadataProvider _metadataProvider;
            
            private MetadataStartPack MetadataStartPack => _metadataProvider.MetadataStartPack ;


            public EntryTransition(VideoMessageCreationScenarioArgs args, ILevelForVideoMessageProvider levelForVideoMessageProvider, TempFileManager tempFileManager, IMetadataProvider metadataProvider) : base(args)
            {
                _levelForVideoMessageProvider = levelForVideoMessageProvider;
                _args = args;
                _tempFileManager = tempFileManager;
                _metadataProvider = metadataProvider;
            }

            public override ScenarioState To => ScenarioState.VideoMessageEditor;

            protected override async Task UpdateContext()
            {
                Context.ExecuteLevelCreationScenario = _args.StartLevelCreationAction;
                Context.OpenedFromChat = _args.OpenedFromChat;
                Context.PublishContext.VideoPublishSettings.MessagePublishInfo = new MessagePublishInfo
                {
                    ShareDestination = _args.ShareDestination
                };
                if (_args.PublishingType.HasValue)
                {
                    Context.PublishContext.PublishingType = _args.PublishingType.Value;
                }else
                {
                    Context.PublishContext.PublishingType = Context.OpenedFromChat != null ? PublishingType.VideoMessage: PublishingType.Post;
                }
                var race = MetadataStartPack.GetRaceByUniverseId(_args.Universe.Id);
                Context.CharacterSelection.Race = race;
                Context.LevelData = await _levelForVideoMessageProvider.GetLevelForVideoMessage(race);
                Context.LevelData.LevelTypeId = ServerConstants.LevelType.VIDEO_MESSAGE;
                await base.UpdateContext();
            }

            protected override Task OnRunning()
            {
                _tempFileManager.RemoveTempFile(Constants.FileDefaultPaths.LEVEL_TEMP_PATH);
                return base.OnRunning();
            }
        }
    }
}