using System;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using Navigation.Args;
using UIManaging.Pages.LevelEditor.Ui.Common;
using static UIManaging.Pages.LevelEditor.Ui.LevelEditorState;


namespace UIManaging.Pages.LevelEditor.Ui
{
    [UsedImplicitly]
    internal sealed class LevelEditorPageModel : BaseEditorPageModel
    {
        private readonly ITemplateProvider _templateProvider;
        public LevelEditorArgs OpeningPageArgs;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<LevelEditorState> StateChanged;
        public event Action LoadingOverlayHidden;
        public event Action ExitButtonClicked;
        public event Action ExitCancelled;
        public event Action MoveToPostRecordEditorClicked;
        public event Action<bool> ExitRequested;
        public event Action EnterVideoMessageEditorRequested;
        public event Action StartOverRequested;
        public event Action<NonLeveVideoData> UploadVideoRequested;
        public event Action OutfitSaveStarted;
        public event Action OutfitSaved;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public LevelEditorState EditorState { get; private set; }
        public LevelEditorState PrevState { get; private set; }

        //---------------------------------------------------------------------
        // Ctor
        //---------------------------------------------------------------------
        
        public LevelEditorPageModel(ILevelManager levelManager, IOutfitFeatureControl outfitFeatureControl, ITemplateProvider templateProvider) 
            : base(levelManager, outfitFeatureControl)
        {
            _templateProvider = templateProvider;
            IsPostRecordEditorOpened = false;
            PrevState = None;
            EditorState = None;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ChangeState(LevelEditorState state)
        {
            PrevState = EditorState;
            EditorState = state;
            StateChanged?.Invoke(state);
        }

        public void ReturnToPrevState()
        {
            ChangeState(PrevState);
        }

        public void OnLoadingOverlayHidden()
        {
            LoadingOverlayHidden?.Invoke();
        }

        public void OnExitButtonClicked()
        {
            ExitButtonClicked?.Invoke();
        }

        public void OnExitCancelled()
        {
            ExitCancelled?.Invoke();
        }

        public void OnMoveToPostRecordEditorClicked()
        {
            MoveToPostRecordEditorClicked?.Invoke();
        }

        public override void OnShoppingCartOpened()
        {
            ChangeState(LevelEditorState.ShoppingCart);
        }

        public override void OnShoppingCartClosed()
        {
            ReturnToPrevState();
        }

        public void RequestExit(bool savedToDraft)
        {
            ExitRequested?.Invoke(savedToDraft);
        }

        public void RequestStartOver()
        {
            StartOverRequested?.Invoke();
        }

        public void RequestNonLevelVideoUploading(NonLeveVideoData videoData)
        {
            UploadVideoRequested?.Invoke(videoData);
        }

        public void RequestOpeningVideoMessageEditor()
        {
            EnterVideoMessageEditorRequested?.Invoke();
        }

        public async Task<bool> HasUserChangedTheOriginalLevel()
        {
            var currentLevel = LevelManager.CurrentLevel;
            if (currentLevel.IsEmpty()) return false;

            var original = OpeningPageArgs.LevelToStartOver;
            if (original == null) return true;
            return !original.IsSavedOnServer() || original.Id != currentLevel.Id ||
                   await LevelManager.IsLevelModified(original, currentLevel);
        }

        public bool HasUserChangedAnyAssetFromOriginalTemplate()
        {
            var targetEvent = LevelManager.TargetEvent;
            var targetTemplate = _templateProvider.GetTemplateEventFromCache(OpeningPageArgs.Template.Id);
            return targetEvent.GetSetLocationId() != targetTemplate.GetSetLocationId() ||
                   targetEvent.GetCharactersCount() != targetTemplate.GetCharactersCount() ||
                   !targetEvent.GetUniqueBodyAnimationIds().OrderBy(x => x)
                              .SequenceEqual(targetTemplate.GetUniqueBodyAnimationIds().OrderBy(x => x));
        }
        
        // TODO: Move to DressUp step context
        public void OnOutfitSaveStarted()
        {
            OutfitSaveStarted?.Invoke();
        }
        
        public void OnOutfitSaved()
        {
            OutfitSaved?.Invoke();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnSongSelectionOpened()
        {
            base.OnSongSelectionOpened();
            ChangeState(SongSelection);
        }

        protected override void OnSongSelectionCloseRequested()
        {
            base.OnSongSelectionCloseRequested();
            ReturnToPrevState();
        }
    }
}