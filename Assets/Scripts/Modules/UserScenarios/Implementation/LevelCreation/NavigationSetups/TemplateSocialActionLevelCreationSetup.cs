using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups
{
    [UsedImplicitly]
    internal sealed class TemplateSocialActionLevelCreationSetup: LevelCreationSetupBase, IResolvable
    {
        public override LevelCreationSetup Type => LevelCreationSetup.TemplateActionSetup;
        
        public TemplateSocialActionLevelCreationSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var templateGridState = ResolveState<TemplateGridState>();
            var characterSelection = ResolveState<CharacterSelectionState>();
            var levelEditorState = ResolveState<LevelEditorState>();
            var postRecordEditorState = ResolveState<PostRecordEditorState>();
            var characterEditor = ResolveState<CharacterEditorState>();
            var publish = ResolveState<PublishState>();
            var publishFromGallery = ResolveState<PublishNonLevelVideoState>();
            var previousPageExit = ResolveState<PreviousPageExitState>();
            var tasksExit = ResolveState<TasksExitState>();
            var feedExit = ResolveState<FeedExitState>();
            var profileExit = ResolveState<ProfileExitState>();
            var homePageExit = ResolveState<HomePageExitState>();
            var inboxPageExit = ResolveState<InboxPageExitState>();
            var chatPageExit = ResolveState<ChatPageExitState>();
            
            templateGridState.MoveNext = ResolveTransition<TemplateGridToLevelEditorTransition>();
            templateGridState.MoveBack = new EmptyTransition(ScenarioState.HomePageExit);
            
            characterSelection.MoveNext = ResolveTransition<CharacterSelectionToLevelEditorTransition>();
            characterSelection.MoveBack = ResolveTransition<LevelEditorToTemplateGridTransition>();

            levelEditorState.MoveNext = ResolveTransition<LevelToPostRecordEditorTransition>();
            levelEditorState.MoveBack = ResolveTransition<LevelEditorToTemplateGridTransition>();
            levelEditorState.DeployFromGallery = GetLevelEditorToNonLevelVideoUploadDefaultTransition();
            levelEditorState.OutfitCreationTransition = ResolveTransition<EnterCharacterEditorTransition>();
            
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

            publish.MoveNextChat = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextPublic = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveNextLimitedAccess = ResolveTransition<ExitScenarioFromPublish>();
            publish.MoveBack = ResolveTransition<BackFromPublishToPostRecordEditorTransition>();
            publish.PreviewRequested = ResolveTransition<PreviewFromPublishTransition>();
            publish.SaveToDraftsRequested = ResolveTransition<ExitFromSaveToDrafts>();
            
            publishFromGallery.OnBack = new EmptyTransition(levelEditorState);
            
            return new IScenarioState[] { templateGridState, characterSelection, levelEditorState, postRecordEditorState, characterEditor, 
                publish, publishFromGallery, previousPageExit, tasksExit, feedExit, profileExit, homePageExit, inboxPageExit, chatPageExit };
        }
    }
}