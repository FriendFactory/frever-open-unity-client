using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups
{
    [UsedImplicitly]
    internal sealed class PiPWithCreationNewEventOnlySetup: LevelCreationSetupBase
    {
        public override LevelCreationSetup Type => LevelCreationSetup.PostRecordWithNewEventCreation;

        public PiPWithCreationNewEventOnlySetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var levelEditorState = ResolveState<LevelEditorState>();
            var characterSelection = ResolveState<CharacterSelectionState>();
            var postRecordEditorState = ResolveState<PostRecordEditorState>();
            var publish = ResolveState<PublishState>();
            var exit = ResolveState<PreviousPageExitState>();
            var inboxPageExit = ResolveState<InboxPageExitState>();
            var chatPageExit = ResolveState<ChatPageExitState>();
            
            characterSelection.MoveNext = ResolveTransition<CharacterSelectionToLevelEditorTaskTransition>();
            characterSelection.MoveBack = new EmptyTransition(exit);
            
            levelEditorState.MoveNext = ResolveTransition<LevelToPostRecordEditorTransition>();
            levelEditorState.MoveBack = levelEditorState.MoveNext;
            levelEditorState.DeployFromGallery = GetLevelEditorToNonLevelVideoUploadDefaultTransition();
            levelEditorState.OutfitCreationTransition = ResolveTransition<EnterCharacterEditorTransition>();
            
            postRecordEditorState.MoveNext = ResolveTransition<PostRecordEditorToPublishTransition>();
            postRecordEditorState.MoveBack = new EmptyTransition(exit);
            postRecordEditorState.OnPreviewFinished = ResolveTransition<BackToPublishAfterPreview>();
            postRecordEditorState.OutfitCreationTransition = ResolveTransition<EnterCharacterEditorTransition>();
            postRecordEditorState.EventCreationTransition = ResolveTransition<EnterPureRecordingTransition>();
            
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = new EmptyTransition(ScenarioState.ProfileExit);

            return new IScenarioState[] { characterSelection, levelEditorState, postRecordEditorState, publish, exit, inboxPageExit, chatPageExit };
        }
    }
}