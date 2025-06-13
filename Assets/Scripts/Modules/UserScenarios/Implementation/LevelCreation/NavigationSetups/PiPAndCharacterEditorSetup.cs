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
    internal sealed class PiPAndCharacterEditorSetup : LevelCreationSetupBase
    {
        public override LevelCreationSetup Type => LevelCreationSetup.PostRecordWithOutfitCreation;

        public PiPAndCharacterEditorSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
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

            characterSelection.MoveNext = ResolveTransition<CharacterSelectionToPostRecordEditorTaskTransition>();
            characterSelection.MoveBack = new EmptyTransition(ScenarioState.PreviousPageExit);
            
            pip.MoveBack = new EmptyTransition(ScenarioState.PreviousPageExit);
            pip.MoveNext = ResolveTransition<PostRecordEditorToPublishTransition>();
            pip.OnPreviewFinished = ResolveTransition<BackToPublishAfterPreview>();
            pip.OutfitCreationTransition = new EnterCharacterEditorTransition(ResolveService<ILevelManager>());

            characterEditor.MoveNext = ResolveTransition<MoveNextFromCharacterEditorTransition>();
            characterEditor.MoveBack = new EmptyTransition(pip);
            
            publish.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextPublic = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = ResolveTransition<ExitFromSaveToDrafts>();

            return new IScenarioState[] {characterSelection, pip, characterEditor, publish, 
                previousPageExit, tasksExit, feedExit, profileExit, inboxPageExit, chatPageExit };
        }
    }
}