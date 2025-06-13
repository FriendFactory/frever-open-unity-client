using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Zenject;
using ChatPageExitState = Modules.UserScenarios.Implementation.Common.ChatPageExitState;
using PublishNonLevelVideoState = Modules.UserScenarios.Implementation.LevelCreation.States.PublishNonLevelVideoState;

namespace Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups
{
    /// <summary>
    /// Setup for PiP(remixing & editing) & Level Editor & Publish & optional Publish From Video gallery scenario flow
    /// </summary>
    [UsedImplicitly]
    internal sealed class RemixSocialActionStatesSetup : LevelCreationSetupBase
    {
        public override LevelCreationSetup Type => LevelCreationSetup.RemixSocialAction;

        public RemixSocialActionStatesSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var levelEditorState = ResolveState<LevelEditorState>();
            var postRecordEditorState = ResolveState<PostRecordEditorState>();
            var characterEditor = ResolveState<CharacterEditorState>();
            var publish = ResolveState<PublishState>();
            var publishFromGallery = ResolveState<PublishNonLevelVideoState>();
            var previousPageExit = ResolveState<PreviousPageExitState>();
            var tasksExit = ResolveState<TasksExitState>();
            var feedExit = ResolveState<FeedExitState>();
            var profileExit = ResolveState<ProfileExitState>();
            var inboxPageExit = ResolveState<InboxPageExitState>();
            var chatPageExit = ResolveState<ChatPageExitState>();
            var exitToVideoMessageScenario = ResolveState<ExitToVideoMessageScenarioState>();
            
            var exitToProfilePage = ResolveExitFromLevelEditor(ScenarioState.ProfileExit);
            var exitToPreviousPage = ResolveExitFromLevelEditor(ScenarioState.PreviousPageExit);
            
            levelEditorState.MoveNext = ResolveTransition<LevelToPostRecordEditorTransition>();
            levelEditorState.MoveBack = new LeaveLevelEditorTransition(new [] {levelEditorState.MoveNext, exitToProfilePage, exitToPreviousPage});
            levelEditorState.DeployFromGallery = GetLevelEditorToNonLevelVideoUploadDefaultTransition();
            levelEditorState.OutfitCreationTransition = ResolveTransition<EnterCharacterEditorTransition>();
            levelEditorState.CreateVideoMessageTransition = ResolveTransition<LevelEditorToVideoMessageScenarioTransition>();
            
            postRecordEditorState.MoveNext = ResolveTransition<PostRecordEditorToPublishTransition>();
            var postRecordToLevelEditor =  ResolveTransition<PostRecordEditorToLevelEditorTransition>();
            var postRecordEditorToProfile = ResolveExitFromPostRecordEditor(ScenarioState.ProfileExit);
            postRecordEditorState.MoveBack =
                new PostRecordEditorMoveBackTransition(new ITransition[] { postRecordToLevelEditor, postRecordEditorToProfile });
            postRecordEditorState.OnPreviewFinished = ResolveTransition<BackToPublishAfterPreview>();
            postRecordEditorState.OutfitCreationTransition = ResolveTransition<EnterCharacterEditorTransition>();
            postRecordEditorState.EventCreationTransition = ResolveTransition<EnterPureRecordingTransition>();
            
            characterEditor.MoveNext = ResolveTransition<MoveNextFromCharacterEditorTransition>();
            characterEditor.MoveBack = ResolveTransition<MoveBackFromCharacterEditorTransition>();
            
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = ResolveTransition<ExitFromSaveToDrafts>();
            publish.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextPublic = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
          
            publishFromGallery.OnBack = new EmptyTransition(levelEditorState);
            publishFromGallery.OnVideoPublishingRequested = ResolveTransition<NonLevelVideoUploadingTransition>();
            
            return new IScenarioState[]
            {
                levelEditorState, postRecordEditorState, characterEditor, publish,
                publishFromGallery, previousPageExit, tasksExit, feedExit, profileExit, inboxPageExit, chatPageExit,
                exitToVideoMessageScenario
            };
        }
    }
}