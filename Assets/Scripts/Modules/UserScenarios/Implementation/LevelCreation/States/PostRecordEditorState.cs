using Bridge;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    internal abstract class PostRecordEditorStateBase : StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly PageManagerHelper _pageManagerHelper;
        private readonly IBridge _bridge;
        private readonly UncompressedBundlesManager _uncompressedBundlesManager;
        private readonly ILevelManager _levelManager;
        
        public ITransition MoveNext;
        public ITransition MoveBack;
        public ITransition OnPreviewFinished;
        public ITransition OutfitCreationTransition;
        public ITransition EventCreationTransition;
        
        public override ScenarioState Type => ScenarioState.PostRecordEditor;

        protected virtual bool ShowHints => true;

        public override ITransition[] Transitions => new[] { MoveNext, MoveBack, OnPreviewFinished, OutfitCreationTransition, EventCreationTransition }.RemoveNulls();

        protected PostRecordEditorStateBase(PageManager pageManager, IBridge bridge,
            UncompressedBundlesManager uncompressedBundlesManager, ILevelManager levelManager, PageManagerHelper pageManagerHelper)
        {
            _pageManager = pageManager;
            _pageManagerHelper = pageManagerHelper;
            _bridge = bridge;
            _uncompressedBundlesManager = uncompressedBundlesManager;
            _levelManager = levelManager;
        }

        public override void Run()
        {
            var pipArgs = new PostRecordEditorArgs
            {
                LevelData = Context.LevelData,
                NavigationMessage = Context.NavigationMessage,
                Settings = Context.PostRecordEditor.PostRecordEditorSettings,
                IsPreviewMode = Context.PostRecordEditor.IsPreviewMode,
                TaskFullInfo = Context.Task,
                CheckIfUserMadeEnoughChangesForTask = Context.PostRecordEditor.CheckIfUserMadeEnoughChangesForTask,
                CheckIfLevelWasModifiedBeforeExit = Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit
            };

            pipArgs.OpeningState.TargetEventSequenceNumber = Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber;
            pipArgs.DecompressBundlesAfterPreview = Context.PostRecordEditor.DecompressBundlesAfterPreview;
            pipArgs.OnMoveForwardRequested += () => MoveNext.Run();
            SetupBackRequest(pipArgs);
            pipArgs.OnPreviewCompleted = () => OnPreviewFinished.Run();
            pipArgs.OnOutfitCreationRequested = OnOutfitCreationRequested;
            pipArgs.OnNewEventCreationRequested = OnNewEventRequested;
            pipArgs.ShowHintsOnDisplay = ShowHints;
            pipArgs.ShowTaskInfo = Context.PostRecordEditor.ShowTaskInfo;
            Context.PostRecordEditor.ShowTaskInfo = false;

            if (Context.PostRecordEditor.ShowPageLoadingPopup)
            {
                _pageManagerHelper.MoveToPiP(pipArgs, true);
                return;
            }
            
            _pageManager.MoveNext(pipArgs);
        }

        protected abstract void SetupBackRequest(PostRecordEditorArgs postRecordEditorArgs);

        protected void OnMovingBack(MovingBackData movingBackData)
        {
            Context.LevelData = movingBackData.LevelData;
            Context.OriginalLevelData = movingBackData.OriginalLevelData;
            Context.SavedAsDraft = movingBackData.SavedAsDraft;
            MoveBack.Run();
        }

        private async void OnOutfitCreationRequested(PiPOutfitCreationRequestData model)
        {
            Context.CharacterEditor.OpenedFrom = Type;
            Context.CharacterEditor.Character = model.TargetCharacter;
            Context.CharacterEditor.Outfit = model.CurrentOutfit;
            Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber = model.TargetEventSequenceNumber;
            await OutfitCreationTransition.Run();
        }
        
        private async void OnNewEventRequested(CreateNewEventRequestData data)
        {
            Context.LevelEditor.TemplateId = data.TemplateId;
            await EventCreationTransition.Run();
        }

        protected void OnCancelLoadingRequested()
        {
            MoveBack.Run();

            _levelManager.CancelLoadingCurrentAssets();
            _bridge.CancelAllFileLoadingProcesses();
            _bridge.ClearCacheWithoutKeyFileStorage();
            _uncompressedBundlesManager.CleanCache();
        }
    }
    
    [UsedImplicitly]
    internal sealed class PostRecordEditorState : PostRecordEditorStateBase
    {
        public PostRecordEditorState(PageManager pageManager, IBridge bridge,
            UncompressedBundlesManager uncompressedBundlesManager, ILevelManager levelManager, 
            PageManagerHelper pageManagerHelper) 
            : base(pageManager, bridge, uncompressedBundlesManager, levelManager, pageManagerHelper)
        {
        }

        protected override void SetupBackRequest(PostRecordEditorArgs postRecordEditorArgs)
        {
            postRecordEditorArgs.CheckIfUserMadeEnoughChangesForTask = true;
            postRecordEditorArgs.CancelLoadingAction = OnCancelLoadingRequested;
            postRecordEditorArgs.OnMovingBackRequested = OnMovingBack;
        }
    }
}