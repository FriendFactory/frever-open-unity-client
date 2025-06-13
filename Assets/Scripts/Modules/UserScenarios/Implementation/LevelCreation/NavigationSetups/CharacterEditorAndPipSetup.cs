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
    internal sealed class CharacterEditorAndPipSetup: LevelCreationSetupBase
    {
        public override LevelCreationSetup Type => LevelCreationSetup.CharacterDressingToPostRecord;
        
        public CharacterEditorAndPipSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var exitState = ResolveState<PreviousPageExitState>();
            var characterSelection = ResolveState<CharacterSelectionState>();
            var pip = ResolveState<PostRecordEditorState>();
            var characterEditor = ResolveState<CharacterEditorState>();
            var publish = ResolveState<PublishState>();
            var previousPageExit = ResolveState<PreviousPageExitState>();
            var tasksExit = ResolveState<TasksExitState>();
            var feedExit = ResolveState<FeedExitState>();
            var profileExit = ResolveState<ProfileExitState>();
            var inboxPageExit = ResolveState<InboxPageExitState>();
            var chatPageExit = ResolveState<ChatPageExitState>();
            
            characterSelection.MoveNext = ResolveTransition<CharacterSelectionToCharacterEditorTaskTransition>();
            characterSelection.MoveBack = new EmptyTransition(ScenarioState.PreviousPageExit);
            
            characterEditor.MoveNext = ResolveTransition<OutfitCreationToPostRecordEditor>();
            characterEditor.MoveBack = new EmptyTransition(ScenarioState.PreviousPageExit);
            
            pip.MoveBack = new EmptyTransition(ScenarioState.ProfileExit);
            pip.MoveNext = ResolveTransition<PostRecordEditorToPublishTransition>();
            pip.OnPreviewFinished = ResolveTransition<BackToPublishAfterPreview>();
            pip.OutfitCreationTransition = new EnterCharacterEditorTransition(ResolveService<ILevelManager>());

            publish.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextPublic = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = new EmptyTransition(ScenarioState.ProfileExit);
            
            return new IScenarioState[] {characterSelection, exitState, pip, characterEditor, publish, previousPageExit, tasksExit, feedExit, profileExit, inboxPageExit, chatPageExit };
        }
    }
}