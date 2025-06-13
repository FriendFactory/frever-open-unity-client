using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups
{
    [UsedImplicitly]
    internal sealed class LevelEditorAndPip: LevelCreationSetupBase
    {
        public override LevelCreationSetup Type => LevelCreationSetup.LevelEditorToPip;

        public LevelEditorAndPip(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var levelEditorState = ResolveState<LevelEditorState>();
            var characterSelection = ResolveState<CharacterSelectionState>();
            var postRecordEditorState = ResolveState<PostRecordEditorState>();
            var publish = ResolveState<PublishState>();
            var previousPageExit = ResolveState<PreviousPageExitState>();
            var tasksExit = ResolveState<TasksExitState>();
            var feedExit = ResolveState<FeedExitState>();
            var profileExit = ResolveState<ProfileExitState>();
            var inboxPageExit = ResolveState<InboxPageExitState>();
            var chatPageExit = ResolveState<ChatPageExitState>();
            
            characterSelection.MoveNext = ResolveTransition<CharacterSelectionToLevelEditorTaskTransition>();
            characterSelection.MoveBack = new EmptyTransition(previousPageExit);
            
            levelEditorState.MoveNext = ResolveTransition<LevelToPostRecordEditorTransition>();
            var exitToPreviousPage = ResolveExitFromLevelEditor(ScenarioState.PreviousPageExit);
            var exitToProfilePage = ResolveExitFromLevelEditor(ScenarioState.ProfileExit);
            levelEditorState.MoveBack = new LeaveLevelEditorTransition(new [] { levelEditorState.MoveNext, exitToPreviousPage, exitToProfilePage });
            levelEditorState.DeployFromGallery = GetLevelEditorToNonLevelVideoUploadDefaultTransition();
            levelEditorState.OutfitCreationTransition = ResolveTransition<EnterCharacterEditorTransition>();
            
            postRecordEditorState.MoveNext = ResolveTransition<PostRecordEditorToPublishTransition>();
            postRecordEditorState.MoveBack = ResolveTransition<PostRecordEditorToLevelEditorTransition>();
            postRecordEditorState.OnPreviewFinished = ResolveTransition<BackToPublishAfterPreview>();
            postRecordEditorState.OutfitCreationTransition = ResolveTransition<EnterCharacterEditorTransition>();
            postRecordEditorState.EventCreationTransition = ResolveTransition<EnterPureRecordingTransition>();
            
            publish.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextPublic = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = new EmptyTransition(ScenarioState.ProfileExit);
            
            return new IScenarioState[] { characterSelection, levelEditorState, postRecordEditorState, publish, 
                previousPageExit, tasksExit, feedExit, profileExit, inboxPageExit, chatPageExit };
        }
    }
}