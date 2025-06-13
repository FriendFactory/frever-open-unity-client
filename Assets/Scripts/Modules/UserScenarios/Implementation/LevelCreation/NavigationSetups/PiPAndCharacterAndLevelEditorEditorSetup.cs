using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using UIManaging.Pages.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups
{
    [UsedImplicitly]
    internal sealed class PiPAndCharacterAndLevelEditorSetup : LevelCreationSetupBase
    {
        public override LevelCreationSetup Type => LevelCreationSetup.PostRecordWithOutfitAndNewEventCreation;

        public PiPAndCharacterAndLevelEditorSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var pip = ResolveState<PostRecordEditorState>();
            var characterEditor = ResolveState<CharacterEditorState>();
            var levelEditorState = ResolveState<LevelEditorState>();
            var publish = ResolveState<PublishState>();
            var previousPageExit = ResolveState<PreviousPageExitState>();
            var tasksExit = ResolveState<TasksExitState>();
            var feedExit = ResolveState<FeedExitState>();
            var profileExit = ResolveState<ProfileExitState>();
            var inboxPageExit = ResolveState<InboxPageExitState>();
            var chatPageExit = ResolveState<ChatPageExitState>();
            
            pip.MoveBack = new EmptyTransition(ScenarioState.PreviousPageExit);
            pip.MoveNext = ResolveTransition<PostRecordEditorToPublishTransition>();
            pip.OnPreviewFinished = ResolveTransition<BackToPublishAfterPreview>();
            pip.OutfitCreationTransition = new EnterCharacterEditorTransition(ResolveService<ILevelManager>());
            pip.EventCreationTransition = ResolveTransition<EnterPureRecordingTransition>();
            
            characterEditor.MoveNext = ResolveTransition<MoveNextFromCharacterEditorTransition>();
            characterEditor.MoveBack = ResolveTransition<MoveBackFromCharacterEditorTransition>();
            
            var exitToProfilePage = ResolveExitFromLevelEditor(ScenarioState.ProfileExit);
            var exitToPreviousPage = ResolveExitFromLevelEditor(ScenarioState.PreviousPageExit);
            levelEditorState.MoveNext = ResolveTransition<LevelToPostRecordEditorTransition>();
            levelEditorState.MoveBack = new LeaveLevelEditorTransition(new[] {levelEditorState.MoveNext, exitToPreviousPage, exitToProfilePage});
            levelEditorState.OutfitCreationTransition = new EnterCharacterEditorTransition(ResolveService<ILevelManager>());
            
            publish.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextPublic = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = ResolveTransition<ExitFromSaveToDrafts>();

            return new IScenarioState[] { pip, characterEditor, publish, 
                previousPageExit, tasksExit, feedExit, profileExit, inboxPageExit, chatPageExit };
        }
    }
}