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
    /// Setup for Level Editor & PiP & Character Editor & Publish & optional Publish From Video gallery scenario flow,
    /// with exit to previous page
    /// </summary>
    [UsedImplicitly]
    internal class DefaultLevelCreationSetup: LevelCreationSetupBase
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override LevelCreationSetup Type => LevelCreationSetup.DefaultLevelCreation;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public DefaultLevelCreationSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override IScenarioState[] SetupStates()
        {
            var characterSelection = ResolveState<CharacterSelectionState>();
            var levelEditorState = ResolveState<LevelEditorState>();
            var postRecordEditorState = ResolveState<PostRecordEditorState>();
            var characterEditor = ResolveState<CharacterEditorState>();
            var publish = ResolveState<PublishState>();
            var publishFromGallery = ResolveState<PublishNonLevelVideoState>();
            var ratingFeed = ResolveState<RatingFeedState>();
            var previousPageExit = ResolveState<PreviousPageExitState>();
            var tasksExit = ResolveState<TasksExitState>();
            var feedExit = ResolveState<FeedExitState>();
            var profileExit = ResolveState<ProfileExitState>();
            var inboxPageExit = ResolveState<InboxPageExitState>();
            var chatPageExit = ResolveState<ChatPageExitState>();
            var crewPageExit = ResolveState<CrewPageExitState>();
            var exitToVideoMessageScenario = ResolveState<ExitToVideoMessageScenarioState>();
            
            characterSelection.MoveNext = ResolveTransition<CharacterSelectionToLevelEditorTransition>();
            characterSelection.MoveBack =  new EmptyTransition(previousPageExit);

            var exitToPreviousPage = ResolveExitFromLevelEditor(ScenarioState.PreviousPageExit);
            var exitToProfilePage = ResolveExitFromLevelEditor(ScenarioState.ProfileExit);
            var exitToFeedPage = ResolveExitFromLevelEditor(ScenarioState.FeedExit);
            levelEditorState.MoveNext = ResolveTransition<LevelToPostRecordEditorTransition>();
            levelEditorState.MoveBack = new LeaveLevelEditorTransition(new []{levelEditorState.MoveNext, exitToPreviousPage, exitToProfilePage, exitToFeedPage});
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

            publish.MoveNextPublic = IsRatingFeedEnabled()
                ? ResolveTransition<PublishToRatingFeedTransition>()
                : ResolveTransition<ExitScenarioFromPublish>();

            publish.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = ResolveTransition<ExitFromSaveToDrafts>();
            
           publishFromGallery.OnBack = new EmptyTransition(levelEditorState);
           publishFromGallery.OnVideoPublishingRequested = ResolveTransition<NonLevelVideoUploadingTransition>();

           ratingFeed.MoveNext = ResolveTransition<ExitScenarioFromPublish>();
           ratingFeed.SkipRating = ResolveTransition<ExitScenarioFromPublish>();
            
            return new IScenarioState[] { characterSelection, levelEditorState, postRecordEditorState, characterEditor, 
                publish, previousPageExit, tasksExit, feedExit, profileExit, inboxPageExit, chatPageExit, exitToVideoMessageScenario,
                crewPageExit, publishFromGallery, ratingFeed
            };
        }
    }
}