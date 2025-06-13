using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups
{
    /// <summary>
    /// Setup for scenario flow which contains only PiP page, such as CAS
    /// </summary>
    /// 
    [UsedImplicitly]
    internal sealed class PiPSetup: LevelCreationSetupBase
    {
        public override LevelCreationSetup Type => LevelCreationSetup.PostRecordOnly;

        public PiPSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var characterSelection = ResolveState<CharacterSelectionState>();
            var pip = ResolveState<PostRecordEditorState>();
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
            
            publish.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextPublic = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = ResolveTransition<ExitFromSaveToDrafts>();

            return new IScenarioState[] { characterSelection, pip, publish, 
                previousPageExit, tasksExit, feedExit, profileExit, inboxPageExit, chatPageExit };
        }
    }
}